using System;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Http;

namespace Platibus.IIS
{
    /// <summary>
    /// Encapsulates the IIS hosted bus and its dependencies for coordinated
    /// intialization and disposal.
    /// </summary>
    public class ManagedBus : IDisposable
    {
        private readonly Task _initialization;

        private readonly IIISConfiguration _configuration;
        private Uri _baseUri;
        private ISubscriptionTrackingService _subscriptionTrackingService;
        private IMessageQueueingService _messageQueueingService;
        private IMessageJournalingService _messageJournalingService;
        private HttpTransportService _transportService;
        private Bus _bus;
        private bool _disposed;

        /// <summary>
        /// Returns the managed bus instance
        /// </summary>
        /// <returns>Returns the managed bus instance</returns>
        public async Task<Bus> GetBus()
        {
            await _initialization;
            return _bus;
        }
        
        /// <summary>
        /// Initializes a new <see cref="ManagedBus"/> with the specified <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">The configuration used to initialize the bus instance
        /// and its related components</param>
        public ManagedBus(IIISConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            _configuration = configuration;
            _initialization = Init();
        }

        private async Task Init(CancellationToken cancellationToken = default(CancellationToken))
        {
            _baseUri = _configuration.BaseUri;
            _subscriptionTrackingService = _configuration.SubscriptionTrackingService;
            _messageQueueingService = _configuration.MessageQueueingService;
            _messageJournalingService = _configuration.MessageJournalingService;
            var endpoints = _configuration.Endpoints;
            _transportService = new HttpTransportService(_baseUri, endpoints, _messageQueueingService, _messageJournalingService, _subscriptionTrackingService);
            _bus = new Bus(_configuration, _baseUri, _transportService, _messageQueueingService);
            await _transportService.Init(cancellationToken);
            await _bus.Init(cancellationToken);
        }

        /// <summary>
        /// Finalizer that ensures resources are released
        /// </summary>
        ~ManagedBus()
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
            if (disposing)
            {
                _bus.TryDispose();
                _transportService.TryDispose();
                _messageQueueingService.TryDispose();
                _messageJournalingService.TryDispose();
                _subscriptionTrackingService.TryDispose();
            }
        }
    }
}