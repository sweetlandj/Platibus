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
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Platibus.RabbitMQ
{
    public class RabbitMQQueue : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(RabbitMQLoggingCategories.RabbitMQ);

        private readonly QueueName _queueName;
        private readonly string _queueExchange;
        private readonly QueueName _retryQueueName;
        private readonly string _retryExchange;

        private readonly IQueueListener _listener;
        private readonly IConnection _connection;
        private readonly Encoding _encoding;
        private readonly bool _autoAcknowledge;
        private readonly int _concurrencyLimit;
        private readonly int _maxAttempts;
        private readonly TimeSpan _retryDelay;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _disposed;
        
        public RabbitMQQueue(QueueName queueName, IQueueListener listener, IConnection connection, Encoding encoding = null, QueueOptions options = default(QueueOptions))
        {
            if (queueName == null) throw new ArgumentNullException("queueName");
            if (listener == null) throw new ArgumentNullException("listener");
            if (connection == null) throw new ArgumentNullException("connection");

            _queueName = queueName;
            _queueExchange = _queueName.GetExchangeName();
            _retryQueueName = queueName.GetRetryQueueName();
            _retryExchange = _queueName.GetRetryExchangeName();

            _listener = listener;
            _connection = connection;
            _encoding = encoding ?? Encoding.UTF8;
            _autoAcknowledge = options.AutoAcknowledge;
            _concurrencyLimit = Math.Max(options.ConcurrencyLimit, 1);
            _maxAttempts = Math.Max(options.MaxAttempts, 1);
            _retryDelay = options.RetryDelay < TimeSpan.Zero ? TimeSpan.Zero : options.RetryDelay;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Init()
        {
            using (var channel = _connection.CreateModel())
            {
                channel.ExchangeDeclare(_queueExchange, "direct", true, false, null);
                channel.QueueDeclare(_queueName, true, false, false, null);
                channel.QueueBind(_queueName, _queueExchange, "", null);

                var retryTtlSeconds = (int) _retryDelay.TotalSeconds;
                var retryQueueArgs = new Dictionary<string, object>
                {
                    {"x-dead-letter-exchange", _queueExchange},
                    {"x-message-ttl", retryTtlSeconds}
                };

                channel.ExchangeDeclare(_retryExchange, "direct", true, false, null);
                channel.QueueDeclare(_retryQueueName, true, false, false, retryQueueArgs);
                channel.QueueBind(_retryQueueName, _retryExchange, "", null);
            }

            for (var i = 0; i < _concurrencyLimit; i++)
            {
                var consumerTag = _queueName + "-" + i;
                StartConsumer(consumerTag, _cancellationTokenSource.Token);
            }
        }

        public Task Enqueue(Message message, IPrincipal principal)
        {
            CheckDisposed();
            return RabbitMQHelper.PublishMessage(message, principal, _connection, null, _queueExchange, _encoding);
        }

        public void Delete()
        {
            CheckDisposed();
            Dispose(true);

            using (var channel = _connection.CreateModel())
            {
                channel.ExchangeDelete(_queueExchange);
                channel.ExchangeDelete(_retryExchange);
                channel.QueueDelete(_queueName);
                channel.QueueDelete(_retryQueueName);
            }
        }

        private void StartConsumer(string consumerTag, CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                using (var channel = _connection.CreateModel())
                {
                    channel.BasicQos(0, 1, false);
                    Log.DebugFormat("RabbitMQ channel number \"{0}\" initialized", channel.ChannelNumber);

                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(_queueName, _autoAcknowledge, consumerTag, consumer);

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var delivery = consumer.Queue.Dequeue();
                        HandleDelivery(channel, delivery, cancellationToken);
                    }
                }
            }, cancellationToken);
        }

        private void HandleDelivery(IModel channel, BasicDeliverEventArgs delivery, CancellationToken cancellationToken)
        {
            try
            {
                // Put on the thread pool to avoid deadlock
                var acknowleged = Task.Run(() => DispatchToListener(delivery, cancellationToken), cancellationToken).Result;
                if (acknowleged)
                {
                    Log.DebugFormat(
                        "Acknowledging message {0} from RabbitMQ queue \"{1}\" on channel {2}...",
                        delivery.DeliveryTag, _queueName, channel.ChannelNumber);

                    channel.BasicAck(delivery.DeliveryTag, false);
                }
                else
                {
                    Log.DebugFormat(
                        "Message {0} from RabbitMQ queue \"{1}\" received on channel {2} not acknowledged",
                        delivery.DeliveryTag, _queueName, channel.ChannelNumber);

                    // Add 1 to the header value because the number of attempts will be zero
                    // on the first try
                    var currentAttempt = delivery.BasicProperties.GetDeliveryAttempts() + 1;
                    if (currentAttempt < _maxAttempts)
                    {
                        Log.DebugFormat(
                            "Re-publishing message {0} to retry queue \"{1}\" for redelivery...",
                            delivery.DeliveryTag, _retryQueueName);

                        var retryProperties = delivery.BasicProperties;
                        retryProperties.IncrementDeliveryAttempts();
                        channel.BasicPublish(_retryExchange, "", retryProperties, delivery.Body);
                    }
                    else
                    {
                        Log.WarnFormat("Maximum delivery attempts for message {0} exceeded.  Sending NACK on channel {1}...",
                               delivery.DeliveryTag, channel.ChannelNumber);
                    }
                    channel.BasicNack(delivery.DeliveryTag, false, false);
                }
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Error processing message {0} from RabbitMQ queue \"{1}\" on channel {2}", e,
                    delivery.DeliveryTag, _queueName, channel.ChannelNumber);

                // Due to the complexity of managing retry counts and delays in
                // RabbitMQ, retries are handled in the HandleDelivery method.
                // Therefore if we get to this point, we've done the best we
                // could.
                channel.BasicNack(delivery.DeliveryTag, false, false);
            }
        }

        private async Task<bool> DispatchToListener(BasicDeliverEventArgs delivery, CancellationToken cancellationToken)
        {
            var messageBody = _encoding.GetString(delivery.Body);
            using (var reader = new StringReader(messageBody))
            using (var messageReader = new MessageReader(reader))
            {
                var principal = await messageReader.ReadPrincipal().ConfigureAwait(false);
                var message = await messageReader.ReadMessage().ConfigureAwait(false);

                var context = new RabbitMQQueuedMessageContext(message.Headers, principal);
                await _listener.MessageReceived(message, context, cancellationToken).ConfigureAwait(false);
                return context.Acknowledged;
            }
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        ~RabbitMQQueue()
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
            if (_disposed) return;

            _cancellationTokenSource.Cancel();
            if (disposing)
            {
                _cancellationTokenSource.Dispose();
            }
        }
    }
}
