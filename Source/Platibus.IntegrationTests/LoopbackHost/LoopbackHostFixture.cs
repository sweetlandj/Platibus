using System;
using System.Threading.Tasks;

namespace Platibus.IntegrationTests.LoopbackHost
{
    public class LoopbackHostFixture : IDisposable
    {
        public static LoopbackHostFixture Instance;
        
        private readonly Task<Platibus.LoopbackHost> _host;

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
            Task.WhenAll(_host.ContinueWith(t => t.Result.TryDispose()))
                .Wait(TimeSpan.FromSeconds(10));
        }
    }
}
