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
using Microsoft.Owin.Hosting;
using Platibus.Http;
using Platibus.Owin;
using Platibus.Utils;

namespace Platibus.IntegrationTests.OwinMiddleware
{
    public class OwinSelfHost : IDisposable
    {
        private HttpTransportService _transportService;
        private IMessageQueueingService _messageQueueingService;
        private ISubscriptionTrackingService _subscriptionTrackingService;
        private PlatibusMiddleware _middleware;
        private IDisposable _webapp;

        private bool _disposed;

        public IBus Bus { get; private set; }

        public static OwinSelfHost Start(string configSectionName, Action<OwinConfiguration> configure = null)
        {
            var owinSelfHost = new OwinSelfHost();
            owinSelfHost.StartAsync(configSectionName, configure).WaitOnCompletionSource();
            return owinSelfHost;
        }

        public Task StartAsync(string configSectionName, Action<OwinConfiguration> configure = null)
        {
            return StartAsync(configSectionName, configuration =>
            {
                configure?.Invoke(configuration);
                return Task.FromResult(0);
            });
        }

        public async Task StartAsync(string configSectionName, Func<OwinConfiguration, Task> configure = null)
        {
            var serverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configSectionName);
            var serverDirectory = new DirectoryInfo(serverPath);
            serverDirectory.Refresh();
            if (serverDirectory.Exists)
            {
                serverDirectory.Delete(true);
            }

            var configuration = new OwinConfiguration();
            var configurationManager = new OwinConfigurationManager();
            await configurationManager.Initialize(configuration, configSectionName);
            await configurationManager.FindAndProcessConfigurationHooks(configuration);

            if (configure != null)
            {
                await configure(configuration);
            }

            var baseUri = configuration.BaseUri;
            _subscriptionTrackingService = configuration.SubscriptionTrackingService;
            _messageQueueingService = configuration.MessageQueueingService;
            
            var transportServiceOptions = new HttpTransportServiceOptions(baseUri, _messageQueueingService, _subscriptionTrackingService)
            {
                DiagnosticService = configuration.DiagnosticService,
                Endpoints = configuration.Endpoints,
                MessageJournal = configuration.MessageJournal,
                BypassTransportLocalDestination = configuration.BypassTransportLocalDestination
            };

            _transportService = new HttpTransportService(transportServiceOptions);

            var bus = new Bus(configuration, baseUri, _transportService, _messageQueueingService);
            _transportService.MessageReceived += (sender, args) => bus.HandleMessage(args.Message, args.Principal);

            await _transportService.Init();
            await bus.Init();

            Bus = bus;

            _middleware = new PlatibusMiddleware(configuration, bus, _transportService);
            _webapp = WebApp.Start(baseUri.ToString(), app => app.UsePlatibusMiddleware(_middleware));
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

            _webapp?.Dispose();
            _middleware?.Dispose();
            _transportService?.Dispose();
            (_messageQueueingService as IDisposable)?.Dispose();
            (_subscriptionTrackingService as IDisposable)?.Dispose();
            (Bus as IDisposable)?.Dispose();
        }
    }
}

#endif