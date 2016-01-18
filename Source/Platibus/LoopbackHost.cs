using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Config;

namespace Platibus
{
    /// <summary>
    /// Simple Platibus host for passing messages within a single process
    /// </summary>
    /// <remarks>
    /// All messages are delivered to the local Platibus instance.  There is no need for
    /// send or subscription rules, although handling rules must still be specified.
    /// </remarks>
    public class LoopbackHost : IDisposable
    {
        /// <summary>
        /// Creates and starts a new loopback host
        /// </summary>
        /// <param name="configSectionName">(Optional) The name of the 
        /// <see cref="PlatibusConfigurationSection"/> to use to configure the </param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be
        /// used by the caller to cancel initialization of the loopback host</param>
        /// <returns>Returns a task whose result will be an initialized loopback host</returns>
        public static async Task<LoopbackHost> Start(string configSectionName = "platibus",
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var configuration = await PlatibusConfigurationManager.LoadLoopbackConfiguration(configSectionName);
            return await Start(configuration, cancellationToken);
        }

        /// <summary>
        /// Creates and starts a new loopback host
        /// </summary>
        /// <param name="configuration">The configuration to use to configure the </param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be
        /// used by the caller to cancel initialization of the loopback host</param>
        /// <returns>Returns a task whose result will be an initialized loopback host</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="configuration"/>
        /// is <c>null</c></exception>
        public static async Task<LoopbackHost> Start(ILoopbackConfiguration configuration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            var host = new LoopbackHost(configuration);
            await host.Init(cancellationToken);
            return host;
        }

        private readonly Bus _bus;
        private bool _disposed;

        /// <summary>
        /// The hosted Platibus instance
        /// </summary>
        /// <returns>Returns the hosted Platibus</returns>
        public IBus Bus
        {
            get { return _bus; }
        }

        private LoopbackHost(ILoopbackConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            // Placeholder value; required by the bus
            var baseUri = configuration.BaseUri;
            var transportService = new LoopbackTransportService(HandleMessage);
            _bus = new Bus(configuration, baseUri, transportService, configuration.MessageQueueingService);
        }

        private async Task Init(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _bus.Init(cancellationToken);
        }

        private Task HandleMessage(Message message, IPrincipal senderPrincipal)
        {
            return _bus.HandleMessage(message, senderPrincipal);
        }

        /// <summary>
        /// Finalizer to ensure that resources are released
        /// </summary>
        ~LoopbackHost()
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
        /// Called by the <see cref="Dispose()"/> method or the finalizer to ensure that
        /// resources are released
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the 
        /// <see cref="Dispose()"/> method (<c>true</c>) or the finalizer (<c>false</c>)</param>
        /// <remarks>
        /// This method will not be called more than once
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_bus")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bus.TryDispose();
            }
        }
    }
}