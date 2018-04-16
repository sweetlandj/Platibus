using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Diagnostics;
using Platibus.Journaling;
using Platibus.Security;

namespace Platibus.RabbitMQ
{
    /// <inheritdoc cref="ITransportService"/>
    /// <inheritdoc cref="IQueueListener"/>
    /// <inheritdoc cref="IDisposable"/>
    /// <summary>
    /// An <see cref="T:Platibus.ITransportService" /> implementation that transfers messages to RabbitMQ brokers
    /// via the Advanced Message Queueing Protocol (AMQP)
    /// </summary>
    public class RabbitMQTransportService : ITransportService, IQueueListener, IDisposable
    {
        private const string InboxQueueName = "inbox";

        private readonly Uri _baseUri;
        private readonly IConnectionManager _connectionManager;
        private readonly IDiagnosticService _diagnosticService;
        private readonly IMessageJournal _messageJournal;
        private readonly ISecurityTokenService _securityTokenService;
        private readonly Encoding _encoding;
        private readonly QueueOptions _defaultQueueOptions;
        private readonly RabbitMQQueue _inboundQueue;
        private readonly ConcurrentDictionary<SubscriptionKey, RabbitMQQueue> _subscriptions = new ConcurrentDictionary<SubscriptionKey, RabbitMQQueue>(); 

        private bool _disposed;
        
        /// <inheritdoc />
        public event TransportMessageEventHandler MessageReceived;

        /// <summary>
        /// Initializes a new <see cref="RabbitMQTransportService"/>
        /// </summary>
        /// <param name="options">The options that govern the configuration and behavior of the
        /// RabbitMQ transport</param>
        public RabbitMQTransportService(RabbitMQTransportServiceOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _baseUri = options.BaseUri;
            _connectionManager = options.ConnectionManager;
            _diagnosticService = options.DiagnosticService ?? DiagnosticService.DefaultInstance;
            _messageJournal = options.MessageJournal;
            _securityTokenService = options.SecurityTokenService ?? new JwtSecurityTokenService();
            _encoding = options.Encoding ?? Encoding.GetEncoding(RabbitMQDefaults.Encoding);
            _defaultQueueOptions = options.DefaultQueueOptions ?? new QueueOptions();

            var connection = _connectionManager.GetConnection(_baseUri);
            var topics = (options.Topics ?? Enumerable.Empty<TopicName>()).Where(t => t != null);
            using (var channel = connection.CreateModel())
            {
                foreach (var topicName in topics)
                {
                    var exchangeName = topicName.GetTopicExchangeName();
                    channel.ExchangeDeclare(exchangeName, "fanout", _defaultQueueOptions.IsDurable, false, new Dictionary<string, object>());
                    _diagnosticService.Emit(
                        new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQExchangeDeclared)
                        {
                            Detail = "Fanout exchange declared for topic",
                            Exchange = exchangeName,
                            Topic = topicName,
                            ChannelNumber = channel.ChannelNumber
                        }.Build());
                }
            }

            _inboundQueue = new RabbitMQQueue(connection, InboxQueueName, this,
                _encoding, _defaultQueueOptions, _diagnosticService, _securityTokenService, null);

            _inboundQueue.Init();
        }

        /// <inheritdoc />
        public async Task ReceiveMessage(Message message, IPrincipal principal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_messageJournal != null)
            {
                await _messageJournal.Append(message, MessageJournalCategory.Received, cancellationToken);
            }

            var messageReceivedHandlers = MessageReceived;
            if (messageReceivedHandlers != null)
            {
                if (_messageJournal != null)
                {
                    await _messageJournal.Append(message, MessageJournalCategory.Received, cancellationToken);
                }

                var args = new TransportMessageEventArgs(message, principal, cancellationToken);
                await messageReceivedHandlers(this, args);
            }
        }
        
        /// <inheritdoc />
        public async Task SendMessage(Message message, IEndpointCredentials credentials = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            CheckDisposed();

            var destination = message.Headers.Destination;
            var connection = _connectionManager.GetConnection(destination);
            using (var channel = connection.CreateModel())
            {
                await RabbitMQHelper.PublishMessage(message, Thread.CurrentPrincipal, channel, InboxQueueName);

                if (_messageJournal != null)
                {
                    await _messageJournal.Append(message, MessageJournalCategory.Sent, cancellationToken);
                }

                await _diagnosticService.EmitAsync(
                    new RabbitMQEventBuilder(this, DiagnosticEventType.MessageDelivered)
                    {
                        Message = message,
                        ChannelNumber = channel.ChannelNumber,
                        Queue = InboxQueueName
                    }.Build(), cancellationToken);
            }
        }
        
        /// <inheritdoc />
        public async Task PublishMessage(Message message, TopicName topicName, CancellationToken cancellationToken)
        {
            CheckDisposed();
            var publisherTopicExchange = topicName.GetTopicExchangeName();
            var connection = _connectionManager.GetConnection(_baseUri);
            using (var channel = connection.CreateModel())
            {
                await RabbitMQHelper.PublishMessage(message, Thread.CurrentPrincipal, channel, null, publisherTopicExchange);

                if (_messageJournal != null)
                {
                    await _messageJournal.Append(message, MessageJournalCategory.Published, cancellationToken);
                }

                await _diagnosticService.EmitAsync(
                    new RabbitMQEventBuilder(this, DiagnosticEventType.MessageDelivered)
                    {
                        Message = message,
                        ChannelNumber = channel.ChannelNumber,
                        Exchange = publisherTopicExchange
                    }.Build(), cancellationToken);
            }
        }

        /// <inheritdoc />
        public Task Subscribe(IEndpoint endpoint, TopicName topicName, TimeSpan ttl, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            var publisherUri = endpoint.Address;
            var subscriptionQueueName = _baseUri.GetSubscriptionQueueName(topicName);
            var subscriptionKey = new SubscriptionKey(publisherUri, subscriptionQueueName);
            _subscriptions.GetOrAdd(subscriptionKey, _ => BindSubscriptionQueue(topicName, publisherUri, subscriptionQueueName, cancellationToken));
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        async Task IQueueListener.MessageReceived(Message message, IQueuedMessageContext context, CancellationToken cancellationToken)
        {
            await ReceiveMessage(message, context.Principal, cancellationToken);
            await context.Acknowledge();
        }

        private RabbitMQQueue BindSubscriptionQueue(TopicName topicName, Uri publisherUri, string subscriptionQueueName, CancellationToken cancellationToken)
        {
            var publisherTopicExchange = topicName.GetTopicExchangeName();           
            var attempts = 0;
            const int maxAttempts = 10;
            var retryDelay = TimeSpan.FromSeconds(5);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var connection = _connectionManager.GetConnection(publisherUri))
                    using (var channel = connection.CreateModel())
                    {
                        attempts++;
                        channel.ExchangeDeclarePassive(publisherTopicExchange);

                        var subscriptionQueue = new RabbitMQQueue(connection,
                            subscriptionQueueName, this, _encoding, _defaultQueueOptions, _diagnosticService, _securityTokenService, null);

                        subscriptionQueue.Init();

                        channel.QueueBind(subscriptionQueueName, publisherTopicExchange, "", null);

                        _diagnosticService.Emit(
                            new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQQueueBound)
                            {
                                Detail = "Subscription queue bound to topic exchange",
                                Queue = subscriptionQueueName,
                                Exchange = publisherTopicExchange,
                                Topic = topicName,
                                ChannelNumber = channel.ChannelNumber
                            }.Build());

                        return subscriptionQueue;
                    }
                }
                catch (Exception ex)
                {
                    if (attempts >= maxAttempts)
                    {
                        throw;
                    }

                    _diagnosticService.Emit(
                        new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQQueueBindError)
                        {
                            Detail = "Error binding subscription queue to topic exchange (attempt " + attempts + " of " + maxAttempts + ").  Retrying in " + retryDelay,
                            Queue = subscriptionQueueName,
                            Exchange = publisherTopicExchange,
                            Topic = topicName,
                            Exception = ex
                        }.Build());
                    
                    Task.Delay(retryDelay, cancellationToken).Wait(cancellationToken);
                }
            }

            throw new OperationCanceledException();
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        /// <summary>
        /// Finalizer to ensure resources are released
        /// </summary>
        ~RabbitMQTransportService()
        {
            Dispose(false);
        }

        /// <inheritdoc />
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
            if (!disposing) return;

            _inboundQueue.Dispose();

            foreach (var subscriptionQueue in _subscriptions)
            {
                subscriptionQueue.Value.Dispose();
            }

            if (_messageJournal is IDisposable disposableMessageJournal)
            {
                disposableMessageJournal.Dispose();
            }

            if (_connectionManager is IDisposable disposableConnectionManager)
            {
                disposableConnectionManager.Dispose();
            }
        }

        private class SubscriptionKey : IEquatable<SubscriptionKey>
        {
            private readonly Uri _publisherUri;
            private readonly QueueName _subscriptionQueueName;

            public SubscriptionKey(Uri publisherUri, QueueName subscriptionQueueName)
            {
                _publisherUri = publisherUri ?? throw new ArgumentNullException(nameof(publisherUri));
                _subscriptionQueueName = subscriptionQueueName ?? throw new ArgumentNullException(nameof(subscriptionQueueName));
            }

            public bool Equals(SubscriptionKey other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(_publisherUri, other._publisherUri) && Equals(_subscriptionQueueName, other._subscriptionQueueName);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as SubscriptionKey);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((_publisherUri != null ? _publisherUri.GetHashCode() : 0)*397) ^ (_subscriptionQueueName != null ? _subscriptionQueueName.GetHashCode() : 0);
                }
            }

            public static bool operator ==(SubscriptionKey left, SubscriptionKey right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(SubscriptionKey left, SubscriptionKey right)
            {
                return !Equals(left, right);
            }
        }
    }
}
