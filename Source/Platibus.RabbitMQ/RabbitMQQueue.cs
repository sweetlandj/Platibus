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
        private readonly string _deadLetterExchange;

        private readonly QueueName _retryQueueName;
        private readonly string _retryExchange;

        private readonly IQueueListener _listener;
        private readonly Encoding _encoding;

        private readonly TimeSpan _ttl;
        private readonly int _maxAttempts;
        private readonly TimeSpan _retryDelay;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly DurableConsumer[] _consumers;
        
        private readonly IConnection _connection;
        private bool _disposed;

        public RabbitMQQueue(QueueName queueName, IQueueListener listener, IConnection connection,
            Encoding encoding = null, QueueOptions options = default(QueueOptions))
        {
            if (queueName == null) throw new ArgumentNullException("queueName");
            if (listener == null) throw new ArgumentNullException("listener");
            if (connection == null) throw new ArgumentNullException("connection");

            _queueName = queueName;
            _queueExchange = _queueName.GetExchangeName();
            _retryQueueName = queueName.GetRetryQueueName();
            _retryExchange = _queueName.GetRetryExchangeName();
            _deadLetterExchange = _queueName.GetDeadLetterExchangeName();

            _listener = listener;
            _connection = connection;
            _encoding = encoding ?? Encoding.UTF8;
            _ttl = options.TTL;
            _maxAttempts = Math.Max(options.MaxAttempts, 1);
            _retryDelay = options.RetryDelay < TimeSpan.Zero ? TimeSpan.Zero : options.RetryDelay;
            _cancellationTokenSource = new CancellationTokenSource();

            var autoAcknowledge = options.AutoAcknowledge;
            var concurrencyLimit = Math.Max(options.ConcurrencyLimit, 1);
            _consumers = new DurableConsumer[concurrencyLimit];
            for (var i = 0; i < _consumers.Length; i++)
            {
                var consumerTag = _queueName + "_" + i;
                _consumers[i] = new DurableConsumer(_connection, queueName, HandleDelivery, consumerTag,
                    autoAcknowledge);
            }
        }

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

                channel.ExchangeDeclare(_queueExchange, "direct", true, false, null);
                channel.ExchangeDeclare(_deadLetterExchange, "direct", true, false, null);
                channel.QueueDeclare(_queueName, true, false, false, queueArgs);
                channel.QueueBind(_queueName, _queueExchange, "", null);

                var retryTtlMs = (int) _retryDelay.TotalMilliseconds;
                var retryQueueArgs = new Dictionary<string, object>
                {
                    {"x-dead-letter-exchange", _queueExchange},
                    {"x-message-ttl", retryTtlMs}
                };

                channel.ExchangeDeclare(_retryExchange, "direct", true, false, null);
                channel.QueueDeclare(_retryQueueName, true, false, false, retryQueueArgs);
                channel.QueueBind(_retryQueueName, _retryExchange, "", null);
            }

            foreach (var consumer in _consumers)
            {
                consumer.Init();
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
                var acknowleged = Task.Run(() => DispatchToListener(delivery, cancellationToken),
                    cancellationToken).Result;

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
                        Log.WarnFormat(
                            "Maximum delivery attempts for message {0} exceeded.  Sending NACK on channel {1}...",
                            delivery.DeliveryTag, channel.ChannelNumber);

                        channel.BasicNack(delivery.DeliveryTag, false, false);
                    }
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
                var principal = await messageReader.ReadPrincipal();
                var message = await messageReader.ReadMessage();

                var context = new RabbitMQQueuedMessageContext(message.Headers, principal);
                await _listener.MessageReceived(message, context, cancellationToken);
                return context.Acknowledged;
            }
        }

        protected void CheckDisposed()
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
                foreach (var consumer in _consumers)
                {
                    consumer.TryDispose();
                }
                _cancellationTokenSource.TryDispose();
            }
        }
    }
}