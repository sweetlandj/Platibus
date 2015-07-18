
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace Platibus.RabbitMQ
{
    public class RabbitMQHost : ITransportService, IQueueListener, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(RabbitMQLoggingCategories.RabbitMQ);

        /// <summary>
        /// Creates and starts a new <see cref="RabbitMQHost"/> based on the configuration
        /// in the named configuration section.
        /// </summary>
        /// <param name="configSectionName">The configuration section containing the
        /// settings for this RabbitMQ hostinstance.</param>
        /// <param name="cancellationToken">(Optional) A cancelation token that may be
        /// used by the caller to interrupt the Rabbit MQ host initialization process</param>
        /// <returns>Returns the fully initialized and listening HTTP server</returns>
        /// <seealso cref="RabbitMQHostConfigurationSection"/>
        public static async Task<RabbitMQHost> Start(string configSectionName = "platibus.rabbitmq",
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var configuration = await RabbitMQHostConfigurationManager.LoadConfiguration(configSectionName);
            return await Start(configuration, cancellationToken);
        }

        /// <summary>
        /// Creates and starts a new <see cref="RabbitMQHost"/> based on the specified
        /// <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The configuration for this HTTP server instance.</param>
        /// <param name="cancellationToken">(Optional) A cancelation token that may be
        /// used by the caller to interrupt the HTTP server initialization process</param>
        /// <returns>Returns the fully initialized and listening HTTP server</returns>
        /// <seealso cref="RabbitMQHostConfigurationSection"/>
        public static async Task<RabbitMQHost> Start(IRabbitMQHostConfiguration configuration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var server = new RabbitMQHost(configuration);
            await server.Init(cancellationToken);
            return server;
        }

        private const string InboxQueueName = "inbox";

        private readonly IConnectionManager _connectionManager;
        private readonly Uri _baseUri;
        private readonly Encoding _encoding;
        private readonly RabbitMQQueue _inboundQueue;
        private readonly Bus _bus;
        private readonly ConcurrentDictionary<SubscriptionKey, RabbitMQQueue> _subscriptions = new ConcurrentDictionary<SubscriptionKey, RabbitMQQueue>(); 

        private bool _disposed;

        public Bus Bus
        {
            get { return _bus; }
        }

        private RabbitMQHost(IRabbitMQHostConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            _baseUri = configuration.BaseUri;
            _connectionManager = new ConnectionManager();
            _encoding = configuration.Encoding ?? Encoding.UTF8;
            
            var messageQueueingService = new RabbitMQMessageQueueingService(_baseUri, _connectionManager, _encoding);

            var inboundQueueOptions = new QueueOptions
            {
                AutoAcknowledge = configuration.AutoAcknowledge,
                MaxAttempts = configuration.MaxAttempts,
                ConcurrencyLimit = configuration.ConcurrencyLimit,
                RetryDelay = configuration.RetryDelay
            };

            var connection = _connectionManager.GetConnection(_baseUri);
            using (var channel = connection.CreateModel())
            {
                foreach (var topicName in configuration.Topics)
                {
                    var exchangeName = topicName.GetTopicExchangeName();
                    Log.DebugFormat("Initializing fanout exchange '{0}' for topic '{1}'...", exchangeName, topicName);
                    channel.ExchangeDeclare(exchangeName, "fanout", true);
                }
            }

            Log.DebugFormat("Initializing inbox queue '{0}'...", InboxQueueName);
            _inboundQueue = new RabbitMQQueue(InboxQueueName, this, connection, _encoding, inboundQueueOptions);
            _bus = new Bus(configuration, configuration.BaseUri, this, messageQueueingService);
        }

        private async Task Init(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _bus.Init(cancellationToken);
            _inboundQueue.Init();
        }

        public async Task MessageReceived(Message message, IQueuedMessageContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            // For now, allow exceptions to propagate and be handled by the RabbitMQQueue
            await _bus.HandleMessage(message, null);
            await context.Acknowledge();
        }

        public async Task SendMessage(Message message, IEndpointCredentials credentials = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            CheckDisposed();
            var destination = message.Headers.Destination;
            var connection = _connectionManager.GetConnection(destination);
            await RabbitMQHelper.PublishMessage(message, Thread.CurrentPrincipal, connection, InboxQueueName);
        }

        public async Task PublishMessage(Message message, TopicName topicName, CancellationToken cancellationToken)
        {
            CheckDisposed();
            var publisherTopicExchange = topicName.GetTopicExchangeName();
            var connection = _connectionManager.GetConnection(_baseUri);
            await RabbitMQHelper.PublishMessage(message, Thread.CurrentPrincipal, connection, null, publisherTopicExchange);
        }

        public Task Subscribe(IEndpoint endpoint, TopicName topicName, TimeSpan ttl, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            var endpointUri = endpoint.Address;
            var subscriptionQueueName = endpointUri.GetSubscriptionQueueName(topicName);
            var subscriptionKey = new SubscriptionKey(endpointUri, subscriptionQueueName);
            _subscriptions.GetOrAdd(subscriptionKey, key =>
            {
                var connection = _connectionManager.GetConnection(endpointUri);
                var publisherTopicExchange = topicName.GetTopicExchangeName();
                
                Log.DebugFormat("Creating subscription queue '{0}'...", subscriptionQueueName);
                var subscriptionQueue = new RabbitMQQueue(subscriptionQueueName, this, connection, _encoding);
                subscriptionQueue.Init();

                using (var channel = connection.CreateModel())
                {
                    Log.DebugFormat("Binding subscription queue '{0}' to topic exchange '{1}'...", subscriptionQueueName, publisherTopicExchange);
                    channel.ExchangeDeclare(publisherTopicExchange, "fanout", true);
                    channel.QueueBind(subscriptionQueueName, publisherTopicExchange, "", null);    
                }
                
                return subscriptionQueue;
            });
            return Task.FromResult(true);
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        ~RabbitMQHost()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(false);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bus.TryDispose();
                foreach (var subscriptionQueue in _subscriptions)
                {
                    subscriptionQueue.TryDispose();
                }
                _inboundQueue.TryDispose();
                _connectionManager.TryDispose();
            }
        }

        private class SubscriptionKey : IEquatable<SubscriptionKey>
        {
            private readonly Uri _publisherUri;
            private readonly QueueName _subscriptionQueueName;

            public SubscriptionKey(Uri publisherUri, QueueName subscriptionQueueName)
            {
                if (publisherUri == null) throw new ArgumentNullException("publisherUri");
                if (subscriptionQueueName == null) throw new ArgumentNullException("subscriptionQueueName");
                _publisherUri = publisherUri;
                _subscriptionQueueName = subscriptionQueueName;
            }

            public bool Equals(SubscriptionKey other)
            {
                if (ReferenceEquals(null, other)) return false;
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
