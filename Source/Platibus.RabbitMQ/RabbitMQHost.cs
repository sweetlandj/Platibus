
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Platibus.RabbitMQ
{
    public class RabbitMQHost : ITransportService, IDisposable
    {
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

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IConnectionFactory _connectionFactory;
        private readonly Encoding _encoding;
        private bool _disposed;

        private RabbitMQHost(IRabbitMQHostConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            _connectionFactory = new ConnectionFactory
            {
                Uri = configuration.ServerUrl.ToString()
            };
            _encoding = configuration.Encoding ?? Encoding.UTF8;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private async Task Init(CancellationToken cancellationToken = default(CancellationToken))
        {
            
        }

        public Task SendMessage(Message message, IEndpointCredentials credentials = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            CheckDisposed();
            throw new NotImplementedException();
        }

        public Task PublishMessage(Message message, TopicName topicName, CancellationToken cancellationToken)
        {
            CheckDisposed();
            throw new NotImplementedException();
        }

        public Task Subscribe(Uri publisherUri, TopicName topicName, TimeSpan ttl, IEndpointCredentials credentials,
            CancellationToken cancellationToken = new CancellationToken())
        {
            CheckDisposed();
            throw new NotImplementedException();
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
                
            }
        }
    }
}
