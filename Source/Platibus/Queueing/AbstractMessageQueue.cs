// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
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
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Platibus.Diagnostics;

namespace Platibus.Queueing
{
    /// <inheritdoc />
    /// <summary>
    /// An abstract base class for implementing message queues
    /// </summary>
    public abstract class AbstractMessageQueue : IDisposable
    {
        /// <summary>
        /// The name of the queue
        /// </summary>
        protected readonly QueueName QueueName;

        /// <summary>
        /// A data sink provided by the implementer that handles diagnostic events emitted from
        /// the message queue 
        /// </summary>
        protected readonly IDiagnosticService DiagnosticService;

        private readonly IQueueListener _listener;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly bool _autoAcknowledge;
        private readonly int _maxAttempts;
        private readonly ActionBlock<QueuedMessage> _queuedMessages;
        private readonly TimeSpan _retryDelay;
        
        private bool _disposed;
        private int _initialized;

        /// <summary>
        /// Event raised when a new message is enqueued
        /// </summary>
        protected MessageQueueEventHandler MessageEnqueued;

        /// <summary>
        /// Event raised when a message is acknowledged by the listener
        /// </summary>
        protected MessageQueueEventHandler MessageAcknowledged;

        /// <summary>
        /// Event raised whenever the listener fails to acknowledge the message
        /// </summary>
        protected MessageQueueEventHandler AcknowledgementFailure;

        /// <summary>
        /// Event raised when the maximum number of attempts is exceeded
        /// </summary>
        protected MessageQueueEventHandler MaximumAttemptsExceeded;

        /// <summary>
        /// Initializes a new <see cref="AbstractMessageQueue"/> with the specified values
        /// </summary>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="listener">The object that will be notified when messages are
        ///     added to the queue</param>
        /// <param name="options">(Optional) Settings that influence how the queue behaves</param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queueName"/>, or 
        /// <paramref name="listener"/> are <c>null</c></exception>
        protected AbstractMessageQueue(QueueName queueName, IQueueListener listener, QueueOptions options = null, IDiagnosticService diagnosticService = null)
        {
            QueueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
            _listener = listener ?? throw new ArgumentNullException(nameof(listener));
            DiagnosticService = diagnosticService ?? Diagnostics.DiagnosticService.DefaultInstance;

            var myOptions = options ?? new QueueOptions();

            _autoAcknowledge = myOptions.AutoAcknowledge;
            _maxAttempts = myOptions.MaxAttempts;
            _retryDelay = myOptions.RetryDelay;

            var concurrencyLimit = myOptions.ConcurrencyLimit;
            _cancellationTokenSource = new CancellationTokenSource();
            _queuedMessages = new ActionBlock<QueuedMessage>(
                async msg => await ProcessQueuedMessage(msg, _cancellationTokenSource.Token),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellationTokenSource.Token,
                    MaxDegreeOfParallelism = concurrencyLimit
                });
        }

        /// <summary>
        /// Reads previously queued messages from the database and initiates message processing
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by the caller
        /// to cancel initialization</param>
        /// <returns>Returns a task that completes when initialization is complete</returns>
        public virtual async Task Init(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Interlocked.Exchange(ref _initialized, 1) == 0)
            {
                try
                {
                    await EnqueueExistingMessages(cancellationToken);
                }
                catch (Exception ex)
                {
                    DiagnosticService.Emit(
                        new DiagnosticEventBuilder(this, DiagnosticEventType.ComponentInitializationError)
                        {
                            Detail = "Error enqueueing previously queued message(s)",
                            Exception = ex
                        }.Build());
                }
            }
        }

        /// <summary>
        /// Read existing messages from the persistent store and 
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by the caller
        /// to cancel the enqueueing operation</param>
        /// <returns></returns>
        protected async Task EnqueueExistingMessages(CancellationToken cancellationToken = default(CancellationToken))
        {
            var pendingMessages = await GetPendingMessages(cancellationToken);
            foreach (var pendingMessage in pendingMessages)
            {
                await _queuedMessages.SendAsync(pendingMessage, cancellationToken);
                await DiagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.MessageRequeued)
                    {
                        Detail = "Persistent message requeued (recovery)",
                        Message = pendingMessage.Message,
                        Queue = QueueName
                    }.Build(), cancellationToken);
            }
        }

        /// <summary>
        /// Selects pending messages from the persistence store
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by the caller
        /// to cancel the fetch operation</param>
        /// <returns>Returns a task whose result is the set of pending messages from the
        /// persistent store</returns>
        protected abstract Task<IEnumerable<QueuedMessage>> GetPendingMessages(CancellationToken cancellationToken = default(CancellationToken));
        
        /// <summary>
        /// Adds a message to the queue
        /// </summary>
        /// <param name="message">The message to add</param>
        /// <param name="principal">The principal that sent the message or from whom
        ///     the message was received</param>
        /// <param name="cancellationToken">A cancellation token that can be used by the caller
        /// to cancel the enqueueing operation</param>
        /// <returns>Returns a task that completes when the message has been added to the queue</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is
        /// <c>null</c></exception>
        /// <exception cref="ObjectDisposedException">Thrown if this SQL message queue instance
        /// has already been disposed</exception>
        public virtual async Task Enqueue(Message message, IPrincipal principal, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            CheckDisposed();

            var queuedMessage = new QueuedMessage(message, principal, 0);
            var handler = MessageEnqueued;
            if (handler != null)
            {
                var args = new MessageQueueEventArgs(QueueName, queuedMessage);
                await MessageEnqueued(this, args);    
            }
            
            await _queuedMessages.SendAsync(queuedMessage, cancellationToken);
            // TODO: handle accepted == false

            await DiagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.MessageEnqueued)
                {
                    Detail = "New message enqueued",
                    Message = message,
                    Queue = QueueName
                }.Build(), cancellationToken);
        }

        /// <summary>
        /// Called by the message processing loop to process an individual message
        /// </summary>
        /// <param name="queuedMessage">The queued message to process</param>
        /// <param name="cancellationToken">A cancellation token that can be used by the caller
        /// to cancel message processing operation</param>
        /// <returns>Returns a task that completes when the queued message is processed</returns>
        protected virtual async Task ProcessQueuedMessage(QueuedMessage queuedMessage,
            CancellationToken cancellationToken)
        {
            Exception exception = null;
            var message = queuedMessage.Message;
            var principal = queuedMessage.Principal;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse

            queuedMessage = queuedMessage.NextAttempt();

            await DiagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.QueuedMessageAttempt)
                {
                    Detail = "Processing queued message (attempt " + queuedMessage.Attempts + " of " + _maxAttempts +
                             ")",
                    Message = message,
                    Queue = QueueName
                }.Build(), cancellationToken);

            var context = new QueuedMessageContext(message, principal);
            Thread.CurrentPrincipal = context.Principal;
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await _listener.MessageReceived(message, context, cancellationToken);
                if (_autoAcknowledge && !context.Acknowledged)
                {
                    await context.Acknowledge();
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                DiagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.UnhandledException)
                {
                    Detail = "Unhandled exception handling queued message",
                    Message = message,
                    Queue = QueueName,
                    Exception = ex
                }.Build());
            }

            var eventArgs = new MessageQueueEventArgs(QueueName, queuedMessage, exception);
            if (context.Acknowledged)
            {
                var messageAcknowledgedHandlers = MessageAcknowledged;
                if (messageAcknowledgedHandlers != null)
                {
                    await messageAcknowledgedHandlers(this, eventArgs);
                }
                return;
            }

            if (queuedMessage.Attempts >= _maxAttempts)
            {
                await DiagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.MaxAttemptsExceeded)
                    {
                        Detail = "Maximum attempts exceeded (" + _maxAttempts + ")",
                        Message = message,
                        Queue = QueueName
                    }.Build(), cancellationToken);

                var maxAttemptsExceededHandlers = MaximumAttemptsExceeded;
                if (maxAttemptsExceededHandlers != null)
                {
                    await MaximumAttemptsExceeded(this, eventArgs);
                }

                return;
            }

            var acknowledgementFailureHandlers = AcknowledgementFailure;
            if (acknowledgementFailureHandlers != null)
            {
                await AcknowledgementFailure(this, eventArgs);
            }

            await DiagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.QueuedMessageRetry)
                {
                    Detail = "Message not acknowledged; retrying in " + _retryDelay,
                    Message = message,
                    Queue = QueueName
                }.Build(), cancellationToken);

            ScheduleRetry(queuedMessage, cancellationToken);
        }

        private void ScheduleRetry(QueuedMessage queuedMessage, CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(() =>
            {
                Task.Delay(_retryDelay, cancellationToken).Wait(cancellationToken);
                if (cancellationToken.IsCancellationRequested) return;
                _queuedMessages.Post(queuedMessage);

                DiagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.MessageRequeued)
                    {
                        Detail = "Message requeued (retry)",
                        Message = queuedMessage.Message,
                        Queue = QueueName
                    }.Build(), cancellationToken);

            },
            TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Throws <see cref="ObjectDisposedException"/> if this object has been disposed
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if this object has been disposed</exception>
        protected void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        /// <summary>
        /// Finalizer to ensure that all resources are released
        /// </summary>
        ~AbstractMessageQueue()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called by the <see cref="Dispose()"/> method or by the finalizer to free held resources
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the 
        /// <see cref="Dispose()"/> method (<c>true</c>) or from the finalizer (<c>false</c>)</param>
        protected virtual void Dispose(bool disposing)
        {
            _cancellationTokenSource.Cancel();
            if (disposing)
            {
                _cancellationTokenSource.Dispose();
            }
        }
    }
}
