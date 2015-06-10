using System;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Http;

namespace Platibus.IIS
{
    public class BusManager : IDisposable, IBusManager
    {
        internal static readonly BusManager SingletonInstance = new BusManager();

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

        public async Task<IBus> GetBus()
        {
            await _initialization;
            return _bus;
        }

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
            _bus = new Bus(_configuration, _baseUri, _transportService);
            await _transportService.Init(cancellationToken);
            await _bus.Init(cancellationToken);
            _resourceRouter = new ResourceTypeDictionaryRouter
            {
                {"message", new MessageController(_bus.HandleMessage)},
                {"topic", new TopicController(_subscriptionTrackingService, _configuration.Topics)}
            };
        }

        ~BusManager()
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