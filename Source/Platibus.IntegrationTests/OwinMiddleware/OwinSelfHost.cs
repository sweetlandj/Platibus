#if NET452
// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Threading.Tasks;
using Platibus.Owin;

namespace Platibus.IntegrationTests.OwinMiddleware
{
    public class OwinSelfHost : IDisposable
    {
        private bool _disposed;

        public IDisposable WebApp { get; }
        public PlatibusMiddleware Middleware { get; }
        public IBus Bus { get; }

        private OwinSelfHost(IDisposable webApp, PlatibusMiddleware middleware, IBus bus)
        {
            WebApp = webApp;
            Middleware = middleware;
            Bus = bus;
        }

        public static async Task<OwinSelfHost> Start(string configSectionName)
        {
            var serverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configSectionName);
            var serverDirectory = new DirectoryInfo(serverPath);
            serverDirectory.Refresh();
            if (serverDirectory.Exists)
            {
                serverDirectory.Delete(true);
            }

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
            if (!disposing) return;
            
            WebApp.Dispose();
            Middleware.Dispose();
            if (Bus is IDisposable disposableBus)
            {
                disposableBus.Dispose();
            }
        }
    }
}

#endif