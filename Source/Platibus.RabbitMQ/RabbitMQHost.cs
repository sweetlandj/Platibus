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

using Platibus.Journaling;
using Platibus.Security;
using Platibus.Utils;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.RabbitMQ
{
    /// <inheritdoc cref="ITransportService" />
    /// <inheritdoc cref="IQueueListener" />
    /// <inheritdoc cref="IDisposable" />
    /// <summary>
    /// Hosts for a bus instance whose queueing and transport are based on RabbitMQ queues
    /// </summary>
    public class RabbitMQHost : IQueueListener, IDisposable
    {
        /// <summary>
        /// Creates and starts a new <see cref="RabbitMQHost"/>
        /// </summary>
        /// <returns>Returns the fully initialized and listening RabbitMQ host</returns>
#if NET452 || NET461
/// <seealso cref="RabbitMQHostConfigurationSection"/> 
#endif
        public static RabbitMQHost Start() => Start(null, _ => { });

        /// <summary>
        /// Creates and starts a new <see cref="RabbitMQHost"/>
        /// </summary>
        /// <param name="configSectionName">The name of the configuration section containing
        /// the RabbitMQ host configuration</param>
        /// <returns>Returns the fully initialized and listening RabbitMQ host</returns>
#if NET452 || NET461
/// <seealso cref="RabbitMQHostConfigurationSection"/> 
#endif
        public static RabbitMQHost Start(string configSectionName) => Start(configSectionName, _ => { });

        /// <summary>
        /// Creates and starts a new <see cref="RabbitMQHost"/>
        /// </summary>
        /// <param name="configure">Delegate used to modify the configuration loaded
        /// from the default configuration section</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by the
        /// caller to cancel RabbitMQ host initialization</param>
        /// <returns>Returns the fully initialized and listening RabbitMQ host</returns>
#if NET452 || NET461
/// <seealso cref="RabbitMQHostConfigurationSection"/> 
#endif
        public static RabbitMQHost Start(
            Action<RabbitMQHostConfiguration> configure, 
            CancellationToken cancellationToken = default(CancellationToken)) => Start(null, configure, cancellationToken);

        /// <summary>
        /// Creates and starts a new <see cref="RabbitMQHost"/>
        /// </summary>
        /// <param name="configure">Delegate used to modify the configuration loaded
        /// from the default configuration section</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by the
        /// caller to cancel RabbitMQ host initialization</param>
        /// <returns>Returns the fully initialized and listening RabbitMQ host</returns>
#if NET452 || NET461
/// <seealso cref="RabbitMQHostConfigurationSection"/> 
#endif
        public static RabbitMQHost Start(
            Func<RabbitMQHostConfiguration, Task> configure, 
            CancellationToken cancellationToken = default(CancellationToken)) => Start(null, configure, cancellationToken);

        /// <summary>
        /// Creates and starts a new <see cref="RabbitMQHost"/>
        /// </summary>
        /// <param name="configSectionName">The name of the configuration section from which
        /// declarative configuration should be loaded before invoking the <paramref name="configure"/></param>
        /// <param name="configure">Delegate used to modify the configuration loaded
        /// from the default configuration section</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by the
        /// caller to cancel RabbitMQ host initialization</param>
        /// <returns>Returns the fully initialized and listening RabbitMQ host</returns>
#if NET452 || NET461
/// <seealso cref="RabbitMQHostConfigurationSection"/> 
#endif
        public static RabbitMQHost Start(
            string configSectionName, 
            Action<RabbitMQHostConfiguration> configure, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Start(configSectionName, configuration =>
            {
                configure?.Invoke(configuration);
                return Task.FromResult(0);
            }, cancellationToken);
        }

        /// <summary>
        /// Creates and starts a new <see cref="RabbitMQHost"/>
        /// </summary>
        /// <param name="configSectionName">The name of the configuration section from which
        /// declarative configuration should be loaded before invoking the 
        /// <paramref name="configure"/></param>
        /// <param name="configure">Delegate used to modify the configuration loaded
        /// from the default configuration section</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by the
        /// caller to cancel RabbitMQ host initialization</param>
        /// <returns>Returns the fully initialized and listening RabbitMQ host</returns>
#if NET452 || NET461
/// <seealso cref="RabbitMQHostConfigurationSection"/> 
#endif
        public static RabbitMQHost Start(
            string configSectionName,
            Func<RabbitMQHostConfiguration, Task> configure, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return StartAsync(configSectionName, configure, cancellationToken)
                .GetResultFromCompletionSource(cancellationToken);
        }

        /// <summary>
        /// Creates and starts a new <see cref="RabbitMQHost"/>
        /// </summary>
        /// <param name="configuration">The HTTP sever configuration</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by the
        /// caller to cancel RabbitMQ host initialization</param>
        /// <returns>Returns the fully initialized and listening RabbitMQ host</returns>
        public static RabbitMQHost Start(IRabbitMQHostConfiguration configuration, CancellationToken cancellationToken = default(CancellationToken))
        {
            return StartAsync(configuration, cancellationToken)
                .GetResultFromCompletionSource(cancellationToken);
        }

        private static async Task<RabbitMQHost> StartAsync(
            string configSectionName,
            Func<RabbitMQHostConfiguration, Task> configure, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var configManager = new RabbitMQHostConfigurationManager();
            var configuration = new RabbitMQHostConfiguration();
            await configManager.Initialize(configuration, configSectionName);
#pragma warning disable 612
            await configManager.FindAndProcessConfigurationHooks(configuration);
#pragma warning restore 612
            if (configure != null)
            {
                await configure(configuration);
            }

            var server = await StartAsync(configuration, cancellationToken);
            return server;
        }

        private static async Task<RabbitMQHost> StartAsync(
            IRabbitMQHostConfiguration configuration, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var server = new RabbitMQHost(configuration);
            await server.Init(cancellationToken);
            return server;
        }
        
        private readonly IConnectionManager _connectionManager;
        private readonly IMessageQueueingService _messageQueueingService;
        private readonly IMessageJournal _messageJournal;
        private readonly RabbitMQTransportService _transportService;

        private bool _disposed;

        /// <summary>
        /// The hosted bus instance
        /// </summary>
        public Bus Bus { get; }

        private RabbitMQHost(IRabbitMQHostConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var diagnosticService = configuration.DiagnosticService;
            var baseUri = configuration.BaseUri.WithoutTrailingSlash();
            _connectionManager = new ConnectionManager();
            var encoding = configuration.Encoding ?? Encoding.UTF8;

            var defaultQueueOptions = new QueueOptions
            {
                AutoAcknowledge = configuration.AutoAcknowledge,
                MaxAttempts = configuration.MaxAttempts,
                ConcurrencyLimit = configuration.ConcurrencyLimit,
                RetryDelay = configuration.RetryDelay,
                IsDurable = configuration.IsDurable
            };

            var securityTokenService = configuration.SecurityTokenService ?? new JwtSecurityTokenService();
            _messageJournal = configuration.MessageJournal;

            var queueingOptions = new RabbitMQMessageQueueingOptions(baseUri)
            {
                ConnectionManager = _connectionManager,
                DefaultQueueOptions = defaultQueueOptions,
                DiagnosticService = diagnosticService,
                Encoding = encoding,
                SecurityTokenService = securityTokenService
            };
            _messageQueueingService = new RabbitMQMessageQueueingService(queueingOptions);
            
            var transportServiceOptions = new RabbitMQTransportServiceOptions(baseUri, _connectionManager)
            {
                DiagnosticService = configuration.DiagnosticService,
                MessageJournal = configuration.MessageJournal,
                SecurityTokenService = configuration.SecurityTokenService,
                Encoding = configuration.Encoding,
                DefaultQueueOptions = defaultQueueOptions,
                Topics = configuration.Topics
            };
            _transportService = new RabbitMQTransportService(transportServiceOptions);

            Bus = new Bus(configuration, configuration.BaseUri, _transportService, _messageQueueingService);
        }

        private async Task Init(CancellationToken cancellationToken = default(CancellationToken))
        {
            await Bus.Init(cancellationToken);
        }

        /// <inheritdoc />
        async Task IQueueListener.MessageReceived(Message message, IQueuedMessageContext context, CancellationToken cancellationToken)
        {
            await _transportService.ReceiveMessage(message, context.Principal, cancellationToken);
            await context.Acknowledge();
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

            _transportService.Dispose();
            Bus.Dispose();

            if (_messageQueueingService is IDisposable disposableMessageQueueingService)
            {
                disposableMessageQueueingService.Dispose();
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
    }
}
