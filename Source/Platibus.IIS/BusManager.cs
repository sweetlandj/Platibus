using System;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Http;

namespace Platibus.IIS
{
    /// <summary>
    /// Initializes an IIS-hosted bus instance
    /// </summary>
    public class BusManager : IDisposable, IBusManager
    {
        internal static readonly BusManager SingletonInstance = new BusManager();

        /// <summary>
        /// Returns the singleton <see cref="IBusManager"/> instance
        /// </summary>
        /// <returns>Returns the singleton <see cref="IBusManager"/> instance</returns>
        public static IBusManager GetInstance()
        {
            return SingletonInstance;
        }

        private readonly Task _initialization;
        private IIISConfiguration _configuration;
        private Uri _baseUri;
        private ISubscriptionTrackingService _subscriptionTrackingService;
        private IMessageQueueingService _messageQueueingService;
        private IMessageJournalingService _messageJournalingService;
        private HttpTransportService _transportService;
        private Bus _bus;
        private IHttpResourceRouter _resourceRouter;
        private bool _disposed;

        /// <summary>
        /// Provides access to the IIS-hosted bus
        /// </summary>
        /// <returns>Returns a task whose result is the bus instance</returns>
        public async Task<IBus> GetBus()
        {
            await _initialization;
            return _bus;
        }

        /// <summary>
        /// Returns a reference to the HTTP resource router
        /// </summary>
        /// <returns>Returns a task whose result is the HTTP resource router</returns>
        public async Task<IHttpResourceRouter> GetResourceRouter()
        {
            await _initialization;
            return _resourceRouter;
        }

        private BusManager()
        {
            _initialization = Init();
        }

        private async Task Init(CancellationToken cancellationToken = default(CancellationToken))
        {
            _configuration = await IISConfigurationManager.LoadConfiguration();
            _baseUri = _configuration.BaseUri;
            _subscriptionTrackingService = _configuration.SubscriptionTrackingService;
            _messageQueueingService = _configuration.MessageQueueingService;
            _messageJournalingService = _configuration.MessageJournalingService;
            var endpoints = _configuration.Endpoints;
            _transportService = new HttpTransportService(_baseUri, endpoints, _messageQueueingService, _messageJournalingService, _subscriptionTrackingService);
            _bus = new Bus(_configuration, _baseUri, _transportService, _messageQueueingService);
            await _transportService.Init(cancellationToken);
            await _bus.Init(cancellationToken);
            _resourceRouter = new ResourceTypeDictionaryRouter
            {
                {"message", new MessageController(_bus.HandleMessage)},
                {"topic", new TopicController(_subscriptionTrackingService, _configuration.Topics)}
            };
        }

        /// <summary>
        /// Finalizer that ensures resources are released
        /// </summary>
        ~BusManager()
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