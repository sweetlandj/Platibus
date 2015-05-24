using System;
using Platibus.Config;
using Platibus.Http;

namespace Platibus.IIS
{
    public class BusManager : IBusHost, IDisposable
    {
        private static readonly BusManager Instance;

        static BusManager()
        {
            var configuration = PlatibusConfigurationManager.LoadConfiguration()
                .ConfigureAwait(false).GetAwaiter().GetResult();

            Instance = new BusManager(configuration);
        }

        public static IBus GetBus()
        {
            return Instance.Bus;
        }

        private readonly Uri _baseUri;
        private readonly ISubscriptionTrackingService _subscriptionTrackingService;
        private readonly HttpTransportService _transportService;
        private readonly Bus _bus;
        private bool _disposed;

        public event MessageReceivedHandler MessageReceived;

        public Uri BaseUri
        {
            get { return _baseUri; }
        }

        public ITransportService TransportService 
        {
            get { return _transportService; }
        }

        public IBus Bus
        {
            get { return _bus; }
        }

        private BusManager(IPlatibusConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            _baseUri = BaseUri;
            _subscriptionTrackingService = configuration.SubscriptionTrackingService;
            _transportService = new HttpTransportService(_baseUri, _subscriptionTrackingService);
            _bus = new Bus(configuration, this);
            _bus.Init().ConfigureAwait(false).GetAwaiter().GetResult();
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
                _subscriptionTrackingService.TryDispose();
            }
        }
    }
}
