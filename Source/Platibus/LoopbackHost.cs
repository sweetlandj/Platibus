using System;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Config;

namespace Platibus
{
    public class LoopbackHost : IDisposable
    {
        public static async Task<LoopbackHost> Start(string configSectionName = "platibus",
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var configuration =
                await PlatibusConfigurationManager.LoadConfiguration<PlatibusConfiguration>(configSectionName);
            return await Start(configuration, cancellationToken);
        }

        public static async Task<LoopbackHost> Start(IPlatibusConfiguration configuration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            var host = new LoopbackHost(configuration);
            await host.Init(cancellationToken);
            return host;
        }

        private readonly Uri _baseUri;
        private readonly Bus _bus;
        private readonly LoopbackTransportService _transportService;
        private bool _disposed;

        public Task<Uri> GetBaseUri()
        {
            return Task.FromResult(_baseUri);
        }

        public Task<IBus> GetBus()
        {
            return Task.FromResult<IBus>(_bus);
        }

        public Task<ITransportService> GetTransportService()
        {
            return Task.FromResult<ITransportService>(_transportService);
        }

        private LoopbackHost(IPlatibusConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            _baseUri = new Uri("http://localhost");
            _bus = new Bus(configuration, _baseUri, _transportService);
            _transportService = new LoopbackTransportService(_bus.HandleMessage);
        }

        private async Task Init(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _bus.Init(cancellationToken);
        }

        ~LoopbackHost()
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