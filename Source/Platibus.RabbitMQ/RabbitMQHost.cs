
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Hosts for a bus instance whose queueing and transport are based on RabbitMQ queues
    /// </summary>
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

        /// <summary>
        /// The hosted bus instance
        /// </summary>
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

        /// <summary>
        /// Sends a message directly to the application identified by the
        /// <see cref="IMessageHeaders.Destination"/> header.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="credentials">The credentials required to send a 
        /// message to the specified destination, if applicable.</param>
        /// <param name="cancellationToken">A token used by the caller to
        /// indicate if and when the send operation has been canceled.</param>
        /// <returns>returns a task that completes when the message has
        /// been successfully sent to the destination.</returns> 
        public async Task SendMessage(Message message, IEndpointCredentials credentials = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            CheckDisposed();
            var destination = message.Headers.Destination;
            var connection = _connectionManager.GetConnection(destination);
            await RabbitMQHelper.PublishMessage(message, Thread.CurrentPrincipal, connection, InboxQueueName);
        }

        /// <summary>
        /// Publishes a message to a topic.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <param name="topicName">The name of the topic.</param>
        /// <param name="cancellationToken">A token used by the caller
        /// to indicate if and when the publish operation has been canceled.</param>
        /// <returns>returns a task that completes when the message has
        /// been successfully published to the topic.</returns>
        public async Task PublishMessage(Message message, TopicName topicName, CancellationToken cancellationToken)
        {
            CheckDisposed();
            var publisherTopicExchange = topicName.GetTopicExchangeName();
            var connection = _connectionManager.GetConnection(_baseUri);
            await RabbitMQHelper.PublishMessage(message, Thread.CurrentPrincipal, connection, null, publisherTopicExchange);
        }

        /// <summary>
        /// Subscribes to messages published to the specified <paramref name="topicName"/>
        /// by the application at the provided <paramref name="endpoint"/>.
        /// </summary>
        /// <param name="endpoint">The publishing endpoint</param>
        /// <param name="topicName">The name of the topic to which the caller is
        ///     subscribing.</param>
        /// <param name="ttl">(Optional) The Time To Live (TTL) for the subscription
        ///     on the publishing application if it is not renewed.</param>
        /// <param name="cancellationToken">A token used by the caller to
        ///     indicate if and when the subscription should be canceled.</param>
        /// <returns>Returns a long-running task that will be completed when the 
        /// subscription is canceled by the caller or a non-recoverable error occurs.</returns>
        public Task Subscribe(IEndpoint endpoint, TopicName topicName, TimeSpan ttl, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            var publisherUri = endpoint.Address;
            var subscriptionQueueName = _baseUri.GetSubscriptionQueueName(topicName);
            var subscriptionKey = new SubscriptionKey(publisherUri, subscriptionQueueName);
            _subscriptions.GetOrAdd(subscriptionKey, key =>
            {
                var connection = _connectionManager.GetConnection(publisherUri);
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

        /// <summary>
        /// Finalizer to ensure resources are released
        /// </summary>
        ~RabbitMQHost()
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
            Dispose(false);
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_bus"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_inboundQueue")]
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
