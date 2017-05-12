using System;
using System.Threading.Tasks;

namespace Platibus.IntegrationTests.LoopbackHost
{
    public class LoopbackHostFixture : IDisposable
    {
        private readonly Task<Platibus.LoopbackHost> _host;

        private bool _disposed;

        public Task<IBus> Bus
        {
            get { return _host.ContinueWith(hostTask => hostTask.Result.Bus); }
        }
        
        public LoopbackHostFixture()
        {
            _host = Platibus.LoopbackHost.Start("platibus.loopback");
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
            Task.WhenAll(_host.ContinueWith(t => t.Result.TryDispose()))
                .Wait(TimeSpan.FromSeconds(10));
        }
    }
}
