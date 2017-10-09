// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
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
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Diagnostics;
using Platibus.Filesystem;
using Platibus.Security;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// A logical Platibus queue implemented with RabbitMQ queues and exchanges
    /// </summary>
    public class RabbitMQQueue : IDisposable
    {
        private readonly QueueName _queueName;
        private readonly string _queueExchange;
        private readonly string _deadLetterExchange;
        private readonly bool _autoAcknowledge;
        private readonly QueueName _retryQueueName;
        private readonly string _retryExchange;
        private readonly IQueueListener _listener;
        private readonly ISecurityTokenService _securityTokenService;
        private readonly Encoding _encoding;
        private readonly TimeSpan _ttl;
        private readonly int _maxAttempts;
        private readonly TimeSpan _retryDelay;
        private readonly bool _isDurable;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly DurableConsumer _consumer;
        private readonly IDiagnosticService _diagnosticService;
        private readonly IConnection _connection;
        private bool _disposed;

        /// <summary>
        /// Initializes a new <see cref="RabbitMQQueue"/>
        /// </summary>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="listener">The listener that will receive new messages off of the queue</param>
        /// <param name="connection">The connection to the RabbitMQ server</param>
        /// <param name="securityTokenService">(Optional) The message security token
        /// service to use to issue and validate security tokens for persisted messages.</param>
        /// <param name="encoding">(Optional) The encoding to use when converting serialized message 
        /// content to byte streams</param>
        /// <param name="options">(Optional) Queueing options</param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queueName"/>, 
        /// <paramref name="listener"/>, or <paramref name="connection"/> is <c>null</c></exception>
        public RabbitMQQueue(QueueName queueName, IQueueListener listener, IConnection connection,
            ISecurityTokenService securityTokenService,
            Encoding encoding = null, QueueOptions options = null, 
            IDiagnosticService diagnosticService = null)
        {
            if (queueName == null) throw new ArgumentNullException("queueName");
            if (listener == null) throw new ArgumentNullException("listener");
            if (connection == null) throw new ArgumentNullException("connection");
            if (securityTokenService == null) throw new ArgumentNullException("securityTokenService");

            _queueName = queueName;
            _queueExchange = _queueName.GetExchangeName();
            _retryQueueName = queueName.GetRetryQueueName();
            _retryExchange = _queueName.GetRetryExchangeName();
            _deadLetterExchange = _queueName.GetDeadLetterExchangeName();

            _listener = listener;
            _connection = connection;
            _securityTokenService = securityTokenService;
            _encoding = encoding ?? Encoding.UTF8;

            var myOptions = options ?? new QueueOptions();

            _ttl = myOptions.TTL;
            _autoAcknowledge = myOptions.AutoAcknowledge;
            _maxAttempts = myOptions.MaxAttempts;
            _retryDelay = myOptions.RetryDelay;
            _isDurable = myOptions.IsDurable;

            var concurrencyLimit = myOptions.ConcurrencyLimit;
            _cancellationTokenSource = new CancellationTokenSource();
            _diagnosticService = diagnosticService ?? DiagnosticService.DefaultInstance;

            var consumerTag = _queueName;
            _consumer = new DurableConsumer(_connection, queueName, HandleDelivery, consumerTag, 
                concurrencyLimit, _autoAcknowledge, _diagnosticService);
        }

        /// <summary>
        /// Initializes RabbitMQ queues and exchanges
        /// </summary>
        public void Init()
        {
            using (var channel = _connection.CreateModel())
            {
                var queueArgs = new Dictionary<string, object>
                {
                    {"x-dead-letter-exchange", _deadLetterExchange},
                };

                if (_ttl > TimeSpan.Zero)
                {
                    queueArgs["x-expires"] = _ttl;
                }

                channel.ExchangeDeclare(_queueExchange, "direct", _isDurable, false, null);
                _diagnosticService.Emit(new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQExchangeDeclared)
                {
                    Detail = "Primary exchange declared for queue",
                    Exchange = _queueExchange,
                    Queue = _queueName
                }.Build());

                channel.ExchangeDeclare(_deadLetterExchange, "direct", _isDurable, false, null);
                _diagnosticService.Emit(new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQExchangeDeclared)
                {
                    Detail = "Dead letter exchange declared for queue",
                    Exchange = _deadLetterExchange,
                    Queue = _queueName
                }.Build());

                channel.QueueDeclare(_queueName, _isDurable, false, false, queueArgs);
                _diagnosticService.Emit(new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQQueueDeclared)
                {
                    Detail = "Queue declared",
                    Queue = _queueName
                }.Build());

                channel.QueueBind(_queueName, _queueExchange, "", null);
                _diagnosticService.Emit(new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQQueueBound)
                {
                    Detail = "Queue bound to primary exchange",
                    Exchange = _queueExchange,
                    Queue = _queueName
                }.Build());

                var retryTtlMs = (int) _retryDelay.TotalMilliseconds;
                var retryQueueArgs = new Dictionary<string, object>
                {
                    {"x-dead-letter-exchange", _queueExchange},
                    {"x-message-ttl", retryTtlMs}
                };

                channel.ExchangeDeclare(_retryExchange, "direct", _isDurable, false, null);
                _diagnosticService.Emit(new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQExchangeDeclared)
                {
                    Detail = "Retry exchange declared for queue",
                    Exchange = _retryExchange,
                    Queue = _queueName
                }.Build());

                channel.QueueDeclare(_retryQueueName, _isDurable, false, false, retryQueueArgs);
                _diagnosticService.Emit(new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQQueueDeclared)
                {
                    Detail = "Retry queue declared",
                    Queue = _retryQueueName
                }.Build());

                channel.QueueBind(_retryQueueName, _retryExchange, "", null);
                _diagnosticService.Emit(new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQQueueDeclared)
                {
                    Detail = "Retry queue bound to retry exchange",
                    Exchange = _retryExchange,
                    Queue = _retryQueueName
                }.Build());
            }

            _consumer.Init();

            _diagnosticService.Emit(new RabbitMQEventBuilder(this, DiagnosticEventType.ComponentInitialization)
            {
                Detail = "RabbitMQ queue initialized",
                Queue = _queueName
            }.Build());
        }

        /// <summary>
        /// Enqueues a message
        /// </summary>
        /// <param name="message">The message to enqueue</param>
        /// <param name="principal">The sender principal</param>
        /// <returns>Returns a task that completes when the message has been enqueued</returns>
        public async Task Enqueue(Message message, IPrincipal principal)
        {
            CheckDisposed();
            var expires = message.Headers.Expires;
            var securityToken = await _securityTokenService.NullSafeIssue(principal, expires);
            var messageWithSecurityToken = message.WithSecurityToken(securityToken);
            using (var channel = _connection.CreateModel())
            {
                await RabbitMQHelper.PublishMessage(messageWithSecurityToken, principal, channel, null, _queueExchange, _encoding);
                await _diagnosticService.EmitAsync(
                    new RabbitMQEventBuilder(this, DiagnosticEventType.MessageEnqueued)
                    {
                        Queue = _queueName,
                        Message = message,
                        ChannelNumber = channel.ChannelNumber
                    }.Build());
            }
        }
        
        /// <summary>
        /// Deletes the RabbitMQ queues and exchanges
        /// </summary>
        public void Delete()
        {
            CheckDisposed();
            Dispose(true);

            using (var channel = _connection.CreateModel())
            {
                channel.QueueDeleteNoWait(_queueName, false, false);
                channel.QueueDeleteNoWait(_retryQueueName, false, false);
                channel.ExchangeDeleteNoWait(_deadLetterExchange, false);
                channel.ExchangeDeleteNoWait(_queueExchange, false);
                channel.ExchangeDeleteNoWait(_retryExchange, false);
            }
        }

        private void HandleDelivery(IModel channel, BasicDeliverEventArgs delivery, CancellationToken cancellationToken)
        {
            try
            {
                // Put on the thread pool to avoid deadlock
                var result = Task.Run(async () => await DispatchToListener(delivery, cancellationToken), cancellationToken).Result;          
                if (result.Acknowledged)
                {
                    _diagnosticService.Emit(
                        new RabbitMQEventBuilder(this, DiagnosticEventType.MessageAcknowledged)
                        {
                            Queue = _queueName,
                            Message = result.Message,
                            ConsumerTag = delivery.ConsumerTag,
                            DeliveryTag = delivery.DeliveryTag
                        }.Build());

                    channel.BasicAck(delivery.DeliveryTag, false);
                }
                else
                {
                    _diagnosticService.Emit(
                        new RabbitMQEventBuilder(this, DiagnosticEventType.MessageNotAcknowledged)
                        {
                            Queue = _queueName,
                            Message = result.Message,
                            ConsumerTag = delivery.ConsumerTag,
                            DeliveryTag = delivery.DeliveryTag
                        }.Build());

                    // Add 1 to the header value because the number of attempts will be zero
                    // on the first try
                    var currentAttempt = delivery.BasicProperties.GetDeliveryAttempts() + 1;
                    if (currentAttempt < _maxAttempts)
                    {
                        _diagnosticService.Emit(
                            new RabbitMQEventBuilder(this, DiagnosticEventType.QueuedMessageRetry)
                            {
                                Detail = "Message not acknowledged; retrying in " + _retryDelay,
                                Message = result.Message,
                                Queue = _queueName,
                                ConsumerTag = delivery.ConsumerTag,
                                DeliveryTag = delivery.DeliveryTag
                            }.Build());

                        var retryProperties = delivery.BasicProperties;
                        retryProperties.IncrementDeliveryAttempts();
                        channel.BasicPublish(_retryExchange, "", retryProperties, delivery.Body);
                    }
                    else
                    {
                        _diagnosticService.Emit(
                            new RabbitMQEventBuilder(this, DiagnosticEventType.MaxAttemptsExceeded)
                            {
                                Message = result.Message,
                                Queue = _queueName,
                                ConsumerTag = delivery.ConsumerTag,
                                DeliveryTag = delivery.DeliveryTag
                            }.Build());
                        
                        channel.BasicNack(delivery.DeliveryTag, false, false);

                        _diagnosticService.Emit(
                            new RabbitMQEventBuilder(this, DiagnosticEventType.DeadLetter)
                            {
                                Message = result.Message,
                                Queue = _queueName,
                                Exchange = _deadLetterExchange,
                                ConsumerTag = delivery.ConsumerTag,
                                DeliveryTag = delivery.DeliveryTag
                            }.Build());
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _diagnosticService.Emit(
                    new RabbitMQEventBuilder(this, DiagnosticEventType.MessageNotAcknowledged)
                    {
                        Detail = "Message not acknowledged due to requested cancelation",
                        Queue = _queueName,
                        ConsumerTag = delivery.ConsumerTag,
                        DeliveryTag = delivery.DeliveryTag
                    }.Build());

                channel.BasicNack(delivery.DeliveryTag, false, false);
            }
            catch (Exception ex)
            {
                _diagnosticService.Emit(
                    new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQDeliveryError)
                    {
                        Detail = "Unhandled exception processing delivery",
                        Exception = ex,
                        Queue = _queueName,
                        ConsumerTag = delivery.ConsumerTag,
                        DeliveryTag = delivery.DeliveryTag
                    }.Build());

                // Due to the complexity of managing retry counts and delays in
                // RabbitMQ, listener exceptions are caught and handled in the
                // DispatchToListener method and publication to the retry
                // exchange is performed within the try block above.  If that
                // fails then there is no other option but to nack the
                // message.
                channel.BasicNack(delivery.DeliveryTag, false, false);
            }
        }

        private async Task<DispatchResult> DispatchToListener(BasicDeliverEventArgs delivery, CancellationToken cancellationToken)
        {
            Message message = null;
            var acknowledged = false;
            try
            {
                var messageBody = _encoding.GetString(delivery.Body);
                using (var reader = new StringReader(messageBody))
                using (var messageReader = new MessageReader(reader))
                {
                    var principal = await messageReader.ReadLegacySenderPrincipal();
                    message = await messageReader.ReadMessage();
                    var securityToken = message.Headers.SecurityToken;
                    if (!string.IsNullOrWhiteSpace(securityToken))
                    {
                        principal = await _securityTokenService.Validate(securityToken);
                    }

                    var headers = new MessageHeaders(message.Headers)
                    {
                        SecurityToken = null
                    };

                    if (headers.Received == default(DateTime))
                    {
                        headers.Received = DateTime.UtcNow;
                    }

                    var context = new RabbitMQQueuedMessageContext(headers, principal);
                    Thread.CurrentPrincipal = context.Principal;
                    await _listener.MessageReceived(message, context, cancellationToken);
                    acknowledged = context.Acknowledged || _autoAcknowledge;
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (MessageFileFormatException ex)
            {
                _diagnosticService.Emit(
                    new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQDeliveryError)
                    {
                        Detail = "Message body could not be parsed",
                        Message = message,
                        Exception = ex,
                        Queue = _queueName,
                        ConsumerTag = delivery.ConsumerTag,
                        DeliveryTag = delivery.DeliveryTag
                    }.Build());
            }
            catch (Exception ex)
            {
                _diagnosticService.Emit(
                    new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQDeliveryError)
                    {
                        Message = message,
                        Exception = ex,
                        Queue = _queueName,
                        ConsumerTag = delivery.ConsumerTag,
                        DeliveryTag = delivery.DeliveryTag
                    }.Build());
            }

            return new DispatchResult(message, acknowledged);
        }

        /// <summary>
        /// Throws an exception if this object has already been disposed
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object has been disposed</exception>
        protected void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        /// <summary>
        /// Finalizer that ensures all resources are released
        /// </summary>
        ~RabbitMQQueue()
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
        /// Called by the <see cref="Dispose()"/> method or finalizer to ensure that
        /// resources are released
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the 
        /// <see cref="Dispose()"/> method (<c>true</c>) or the finalizer (<c>false</c>)</param>
        /// <remarks>
        /// This method will not be called more than once
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            _cancellationTokenSource.Cancel();
            if (disposing)
            {
                _cancellationTokenSource.Dispose();
                _consumer.Dispose();
            }
        }
    }
}