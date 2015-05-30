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

namespace Platibus.Filesystem
{
    internal class FilesystemMessageQueue : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Filesystem);

        private bool _disposed;
        private int _initialized;
        private readonly bool _autoAcknowledge;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly SemaphoreSlim _concurrentMessageProcessingSlot;
        private readonly DirectoryInfo _directory;
        private readonly DirectoryInfo _deadLetterDirectory;
        private readonly IQueueListener _listener;
        private readonly int _maxAttempts;
        private readonly BufferBlock<MessageFile> _queuedMessages;
        private readonly TimeSpan _retryDelay;
        private Task _processingTask;

        public FilesystemMessageQueue(DirectoryInfo directory, IQueueListener listener,
            QueueOptions options = default(QueueOptions))
        {
            if (directory == null) throw new ArgumentNullException("directory");
            if (listener == null) throw new ArgumentNullException("listener");

            _directory = directory;
            _deadLetterDirectory = new DirectoryInfo(Path.Combine(directory.FullName, "dead"));

            _listener = listener;
            _autoAcknowledge = options.AutoAcknowledge;
            _maxAttempts = options.MaxAttempts <= 0 ? 10 : options.MaxAttempts;
            _retryDelay = options.RetryDelay < TimeSpan.Zero ? TimeSpan.Zero : options.RetryDelay;

            var concurrencyLimit = options.ConcurrencyLimit <= 0
                ? QueueOptions.DefaultConcurrencyLimit
                : options.ConcurrencyLimit;
            _concurrentMessageProcessingSlot = new SemaphoreSlim(concurrencyLimit);

            _cancellationTokenSource = new CancellationTokenSource();
            _queuedMessages = new BufferBlock<MessageFile>(new DataflowBlockOptions
            {
                CancellationToken = _cancellationTokenSource.Token
            });
        }

        public async Task Enqueue(Message message, IPrincipal senderPrincipal, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            var queuedMessage = await MessageFile.Create(_directory, message, senderPrincipal, cancellationToken);
            await _queuedMessages.SendAsync(queuedMessage, cancellationToken);
            // TODO: handle accepted == false
        }

        public async Task Init(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Interlocked.Exchange(ref _initialized, 1) == 0)
            {
                _directory.Refresh();
                var directoryAlreadyExisted = _directory.Exists;
                if (!directoryAlreadyExisted)
                {
                    _directory.Create();
                    _directory.Refresh();
                }

                _deadLetterDirectory.Refresh();
                if (!_deadLetterDirectory.Exists)
                {
                    _deadLetterDirectory.Create();
                    _deadLetterDirectory.Refresh();
                }

                if (directoryAlreadyExisted)
                {
                    await EnqueueExistingFiles(cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();
                _processingTask = ProcessQueuedMessages(_cancellationTokenSource.Token);
            }
        }

        private async Task EnqueueExistingFiles(CancellationToken cancellationToken)
        {
            var files = _directory.EnumerateFiles();
            foreach (var file in files)
            {
                Log.DebugFormat("Enqueueing existing message from file {0}...", file);
                var queuedMessage = new MessageFile(file);
                await _queuedMessages.SendAsync(queuedMessage, cancellationToken);
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private async Task ProcessQueuedMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var nextQueuedMessage = await _queuedMessages.ReceiveAsync(cancellationToken);

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
            var deadLetter = false;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            while (!deadLetter && attemptCount < _maxAttempts)
            {
                attemptCount++;

                Log.DebugFormat("Processing queued message {0} (attempt {1} of {2})...",
                    queuedMessage.File,
                    attemptCount,
                    _maxAttempts);

                var context = new FilesystemQueuedMessageContext(queuedMessage);
                cancellationToken.ThrowIfCancellationRequested();

                await _concurrentMessageProcessingSlot.WaitAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var message = await queuedMessage.ReadMessage(cancellationToken);
                    await _listener.MessageReceived(message, context, cancellationToken);
                    if (_autoAcknowledge && !context.Acknowledged)
                    {
                        await context.Acknowledge();
                    }
                }
                catch (MessageFileFormatException ex)
                {
                    Log.ErrorFormat("Unable to read invalid or corrupt message file {0}", ex, ex.Path);
                    deadLetter = true;
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
                    await queuedMessage.Delete(cancellationToken);
                    Log.DebugFormat("Message file {0} deleted successfully", queuedMessage.File);
                    return;
                }

                if (attemptCount >= _maxAttempts)
                {
                    Log.WarnFormat("Maximum attempts to proces message file {0} exceeded", queuedMessage.File);
                    deadLetter = true;
                }

                if (deadLetter)
                {
                    await queuedMessage.MoveTo(_deadLetterDirectory, cancellationToken);
                    return;
                }

                Log.DebugFormat("Message not acknowledged.  Retrying in {0}...", _retryDelay);
                await Task.Delay(_retryDelay, cancellationToken);
            }
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        ~FilesystemMessageQueue()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _cancellationTokenSource.Cancel();
            _processingTask.TryWait(TimeSpan.FromSeconds(30));
            if (disposing)
            {
                _concurrentMessageProcessingSlot.Dispose();
                _cancellationTokenSource.Dispose();
            }
        }
    }
}