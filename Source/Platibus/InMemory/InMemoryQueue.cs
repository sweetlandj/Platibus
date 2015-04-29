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
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Common.Logging;

namespace Platibus.InMemory
{
    class InMemoryQueue : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Filesystem);
        private bool _disposed;
        private int _initialized;
        private readonly bool _autoAcknowledge;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly SemaphoreSlim _concurrentMessageProcessingSlot;
        private readonly IQueueListener _listener;
        private readonly int _maxAttempts;
        private readonly BufferBlock<QueuedMessage> _queuedMessages = new BufferBlock<QueuedMessage>();
        private readonly TimeSpan _retryDelay;

        public InMemoryQueue(IQueueListener listener, QueueOptions options = default(QueueOptions))
        {
            if (listener == null) throw new ArgumentNullException("listener");

            _listener = listener;
            _autoAcknowledge = options.AutoAcknowledge;
            _maxAttempts = options.MaxAttempts <= 0 ? int.MaxValue : options.MaxAttempts;
            _retryDelay = options.RetryDelay < TimeSpan.Zero ? TimeSpan.Zero : options.RetryDelay;

            var concurrencyLimit = options.ConcurrencyLimit <= 0
                ? QueueOptions.DefaultConcurrencyLimit
                : options.ConcurrencyLimit;
            _concurrentMessageProcessingSlot = new SemaphoreSlim(concurrencyLimit);
        }

        public Task Enqueue(Message message, IPrincipal senderPrincipal)
        {
            CheckDisposed();
            var queuedMessage = new QueuedMessage(message, senderPrincipal);
            return _queuedMessages.SendAsync(queuedMessage);
            // TODO: handle accepted == false
        }

        public void Init()
        {
            if (Interlocked.Exchange(ref _initialized, 1) == 0)
            {
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
        private async Task ProcessQueuedMessage(QueuedMessage queuedMessage, CancellationToken cancellationToken)
        {
            var attemptsRemaining = _maxAttempts;
            while (attemptsRemaining > 0)
            {
                attemptsRemaining--;
                var message = queuedMessage.Message;
                var context = new InMemoryQueuedMessageContext(message, queuedMessage.SenderPrincipal);
                cancellationToken.ThrowIfCancellationRequested();

                await _concurrentMessageProcessingSlot.WaitAsync(cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await _listener.MessageReceived(message, context, cancellationToken).ConfigureAwait(false);
                    if (_autoAcknowledge && !context.Acknowledged)
                    {
                        await context.Acknowledge().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    Log.WarnFormat("Unhandled exception handling queued message {0}", ex, queuedMessage.Message.Headers.MessageId);
                }
                finally
                {
                    _concurrentMessageProcessingSlot.Release();
                }

                if (context.Acknowledged)
                {
                    // TODO: Implement journaling
                    break;
                }
                if (attemptsRemaining > 0)
                {
                    await Task.Delay(_retryDelay, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        ~InMemoryQueue()
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
            if (disposing)
            {
                _concurrentMessageProcessingSlot.Dispose();
                _cancellationTokenSource.Dispose();
            }
        }

        private class QueuedMessage
        {
            private readonly Message _message;
            private readonly IPrincipal _senderPrincipal;

            public Message Message
            {
                get { return _message; }
            }

            public IPrincipal SenderPrincipal
            {
                get { return _senderPrincipal; }
            }

            public QueuedMessage(Message message, IPrincipal senderPrincipal)
            {
                if (message == null) throw new ArgumentNullException("message");
                _message = message;
                _senderPrincipal = senderPrincipal;
            }
        }
    }
}
