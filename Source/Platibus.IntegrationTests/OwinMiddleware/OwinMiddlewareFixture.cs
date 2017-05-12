using System;
using System.Threading.Tasks;

namespace Platibus.IntegrationTests.OwinMiddleware
{
    public class OwinMiddlewareFixture : IDisposable
    {
        private readonly Task<OwinSelfHost> _sendingHost;
        private readonly Task<OwinSelfHost> _receivingHost;

        public Task<IBus> Sender
        {
            get { return _sendingHost.ContinueWith(hostTask => hostTask.Result.Bus); }
        }

        public Task<IBus> Receiver
        {
            get { return _receivingHost.ContinueWith(hostTask => hostTask.Result.Bus); }
        }

        public OwinMiddlewareFixture()
        {
            _sendingHost = OwinSelfHost.Start("platibus.owin0");
            _receivingHost = OwinSelfHost.Start("platibus.owin1");
        }

        public void Dispose()
        {
            Task.WhenAll(
                    _sendingHost.ContinueWith(t => t.Result.TryDispose()),
                    _receivingHost.ContinueWith(t => t.Result.TryDispose()))
                .Wait(TimeSpan.FromSeconds(10));
        }
    }
}
