// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Common.Logging;

namespace Pluribus.Filesystem
{
    internal class FilesystemQueue : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Filesystem);

        private bool _disposed;
        private int _initialized;
        private readonly bool _autoAcknowledge;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly SemaphoreSlim _concurrentMessageProcessingSlot;
        private readonly DirectoryInfo _directory;
        private readonly IQueueListener _listener;
        private readonly int _maxAttempts;
        private readonly BufferBlock<MessageFile> _queuedMessages = new BufferBlock<MessageFile>();
        private readonly TimeSpan _retryDelay;

        public FilesystemQueue(DirectoryInfo directory, IQueueListener listener,
            QueueOptions options = default(QueueOptions))
        {
            if (directory == null) throw new ArgumentNullException("directory");
            if (listener == null) throw new ArgumentNullException("listener");

            _directory = directory;
            _listener = listener;
            _autoAcknowledge = options.AutoAcknowledge;
            _maxAttempts = options.MaxAttempts <= 0 ? 10 : options.MaxAttempts;
            _retryDelay = options.RetryDelay < TimeSpan.Zero ? TimeSpan.Zero : options.RetryDelay;

            var concurrencyLimit = options.ConcurrencyLimit <= 0
                ? QueueOptions.DefaultConcurrencyLimit
                : options.ConcurrencyLimit;
            _concurrentMessageProcessingSlot = new SemaphoreSlim(concurrencyLimit);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task Enqueue(Message message, IPrincipal senderPrincipal)
        {
            CheckDisposed();

            var queuedMessage = await MessageFile.Create(_directory, message, senderPrincipal).ConfigureAwait(false);
            await _queuedMessages.SendAsync(queuedMessage).ConfigureAwait(false);
            // TODO: handle accepted == false
        }

        private async Task EnqueueExistingFiles()
        {
            var files = _directory.EnumerateFiles();
            foreach (var file in files)
            {
                Log.DebugFormat("Enqueueing existing message from file {0}...", file);
                var queuedMessage = new MessageFile(file);
                await _queuedMessages.SendAsync(queuedMessage).ConfigureAwait(false);
            }
        }

        public async Task Init()
        {
            if (Interlocked.Exchange(ref _initialized, 1) == 0)
            {
                _directory.Refresh();
                if (!_directory.Exists)
                {
                    _directory.Create();
                    _directory.Refresh();
                }
                else
                {
                    await EnqueueExistingFiles().ConfigureAwait(false);
                }
                // ReSharper disable once UnusedVariable
                var processingTask = ProcessQueuedMessages(_cancellationTokenSource.Token);
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private async Task ProcessQueuedMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var nextQueuedMessage = await _queuedMessages.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                
                // We don't want to wait on this task; we want to allow concurrent processing
                // of messages.  The semaphore will be released by the ProcessQueuedMessage
                // method.

                // ReSharper disable once UnusedVariable
                var messageProcessingTask = ProcessQueuedMessage(nextQueuedMessage, cancellationToken);
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private async Task ProcessQueuedMessage(MessageFile queuedMessage, CancellationToken cancellationToken)
        {
            var attemptCount = 0;
            var attemptsRemaining = _maxAttempts;
            while (attemptsRemaining > 0)
            {
                attemptCount++;
                attemptsRemaining--;
                Log.DebugFormat("Processing queued message {0} (attempt {1} of {2})...", queuedMessage.File,
                    attemptCount, _maxAttempts);
                
                var context = new FilesystemQueuedMessageContext(queuedMessage);
                cancellationToken.ThrowIfCancellationRequested();

                await _concurrentMessageProcessingSlot.WaitAsync(cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var message = await queuedMessage.ReadMessage(cancellationToken).ConfigureAwait(false);
                    await _listener.MessageReceived(message, context, cancellationToken).ConfigureAwait(false);
                    if (_autoAcknowledge && !context.Acknowledged)
                    {
                        await context.Acknowledge().ConfigureAwait(false);
                    }
                }
                catch (MessageFileFormatException ex)
                {
                    Log.ErrorFormat("Unable to read invalid or corrupt message file {0}", ex, ex.Path);
                    break;
                }
                catch (Exception ex)
                {
                    Log.WarnFormat("Unhandled exception handling queued message file {0}", ex, queuedMessage.File);
                }
                finally
                {
                    _concurrentMessageProcessingSlot.Release();
                }

                if (context.Acknowledged)
                {
                    Log.DebugFormat("Message acknowledged.  Deleting message file {0}...", queuedMessage.File);
                    // TODO: Implement journaling
                    queuedMessage.File.Delete();
                    Log.DebugFormat("Message file {0} deleted successfully", queuedMessage.File);
                    break;
                }

                if (attemptsRemaining > 0)
                {
                    Log.DebugFormat("Message not acknowledged.  Retrying in {0}...", _retryDelay);
                    await Task.Delay(_retryDelay, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        ~FilesystemQueue()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _cancellationTokenSource.Cancel();
            if (disposing)
            {
                _concurrentMessageProcessingSlot.Dispose();
                _cancellationTokenSource.Dispose();
            }
            _disposed = true;
        }
    }
}