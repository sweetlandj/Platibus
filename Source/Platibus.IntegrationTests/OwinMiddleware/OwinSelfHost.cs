using System;
using System.Threading.Tasks;
using Platibus.Owin;

namespace Platibus.IntegrationTests.OwinMiddleware
{
    public class OwinSelfHost : IDisposable
    {
        private readonly IDisposable _webApp;
        private readonly PlatibusMiddleware _middleware;
        private readonly IBus _bus;

        private bool _disposed;

        public IDisposable WebApp { get { return _webApp; } }
        public PlatibusMiddleware Middleware { get { return _middleware; } }
        public IBus Bus { get { return _bus; } }

        private OwinSelfHost(IDisposable webApp, PlatibusMiddleware middleware, IBus bus)
        {
            _webApp = webApp;
            _middleware = middleware;
            _bus = bus;
        }

        public static async Task<OwinSelfHost> Start(string configSectionName)
        {
            var middleware = new PlatibusMiddleware(configSectionName);
            var configuration = await middleware.Configuration;
            var baseUri = configuration.BaseUri;
            var webApp = Microsoft.Owin.Hosting.WebApp.Start(baseUri.ToString(), app => app.UsePlatibusMiddleware(middleware));
            var bus = await middleware.Bus;
            return new OwinSelfHost(webApp, middleware, bus);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _webApp.TryDispose();
            _bus.TryDispose();
            _middleware.TryDispose();
        }
    }
}
