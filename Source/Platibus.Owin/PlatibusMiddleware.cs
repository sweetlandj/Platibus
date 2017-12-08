// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
using Platibus.Diagnostics;
using Platibus.Http;
using Platibus.Http.Controllers;
using Platibus.Journaling;

namespace Platibus.Owin
{
    public class PlatibusMiddleware : IDisposable
    {
        private readonly HttpMetricsCollector _metricsCollector = new HttpMetricsCollector();
        private readonly Task<IHttpResourceRouter> _resourceRouter;

        private HttpTransportService _transportService;
        private ISubscriptionTrackingService _subscriptionTrackingService;
        private IMessageQueueingService _messageQueueingService;
        private IMessageJournal _messageJournal;

        private bool _disposed;

        public Task<IOwinConfiguration> Configuration { get; }

        public Task<Bus> Bus { get; }

        public PlatibusMiddleware(string sectionName = null)
            : this(LoadConfiguration(sectionName))
        {
        }

        public PlatibusMiddleware(IOwinConfiguration configuration) 
            : this(Task.FromResult(configuration))
        {
        }

        public PlatibusMiddleware(Task<IOwinConfiguration> configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            Configuration = Configure(configuration);
            Bus = InitBus(Configuration);
            _resourceRouter = InitResourceRouter(Configuration, Bus);
        }

        private async Task<IOwinConfiguration> Configure(Task<IOwinConfiguration> loadConfiguration)
        {
            var configuration = await loadConfiguration;
            configuration.DiagnosticService.AddSink(_metricsCollector);
            return configuration;
        }
        
        public async Task Invoke(IOwinContext context, Func<Task> next)
        {
            var configuration = await Configuration ?? new OwinConfiguration();
            var baseUri = configuration.BaseUri;
            var bus = await Bus;
            context.SetBus(bus);

            if (IsPlatibusUri(context.Request.Uri, baseUri))
            {
                await HandlePlatibusRequest(context, configuration.DiagnosticService);
            }
            else if (next != null)
            {
                await next();
            }
        }

        private async Task HandlePlatibusRequest(IOwinContext context, IDiagnosticService diagnosticService)
        {
            await diagnosticService.EmitAsync(
                new HttpEventBuilder(this, HttpEventType.HttpRequestReceived)
                {
                    Remote = context.Request.RemoteIpAddress,
                    Uri = context.Request.Uri,
                    Method = context.Request.Method
                }.Build());

            var resourceRequest = new OwinRequestAdapter(context.Request);
            var resourceResponse = new OwinResponseAdapter(context.Response);

            try
            {
                var router = await _resourceRouter;
                await router.Route(resourceRequest, resourceResponse);
            }
            catch (Exception ex)
            {
                var exceptionHandler = new HttpExceptionHandler(resourceRequest, resourceResponse, diagnosticService);
                exceptionHandler.HandleException(ex);
            }

            await diagnosticService.EmitAsync(
                new HttpEventBuilder(this, HttpEventType.HttpResponseSent)
                {
                    Remote = context.Request.RemoteIpAddress,
                    Uri = context.Request.Uri,
                    Method = context.Request.Method,
                    Status = context.Response.StatusCode
                }.Build());
        }

        private static async Task<IOwinConfiguration> LoadConfiguration(string sectionName)
        {
            var configManager = new OwinConfigurationManager();
            var configuration = new OwinConfiguration();
            await configManager.Initialize(configuration, sectionName);
            await configManager.FindAndProcessConfigurationHooks(configuration);
            return configuration;
        }

        private async Task<Bus> InitBus(Task<IOwinConfiguration> configuration)
        {
            return await InitBus(await configuration);
        }

        private async Task<Bus> InitBus(IOwinConfiguration configuration)
        {
            var baseUri = configuration.BaseUri;
            
            _subscriptionTrackingService = configuration.SubscriptionTrackingService;
            _messageQueueingService = configuration.MessageQueueingService;
            _messageJournal = configuration.MessageJournal;
            
            var endpoints = configuration.Endpoints;
            _transportService = new HttpTransportService(baseUri, endpoints, 
                _messageQueueingService,
                _messageJournal, _subscriptionTrackingService,
                configuration.BypassTransportLocalDestination, 
                HandleMessage, 
                configuration.DiagnosticService);

            var bus = new Bus(configuration, baseUri, _transportService, _messageQueueingService);

            await _transportService.Init();
            await bus.Init();

            return bus;
        }

        private async Task<IHttpResourceRouter> InitResourceRouter(Task<IOwinConfiguration> configuration, Task<Bus> bus)
        {
            return InitResourceRouter(await configuration, await bus);
        }

        private IHttpResourceRouter InitResourceRouter(IOwinConfiguration configuration, Bus bus)
        {
            var authorizationService = configuration.AuthorizationService;
            var subscriptionTrackingService = configuration.SubscriptionTrackingService;
            return new ResourceTypeDictionaryRouter
            {
                {"message", new MessageController(bus.HandleMessage, authorizationService)},
                {"topic", new TopicController(subscriptionTrackingService, configuration.Topics, authorizationService)},
                {"journal", new JournalController(configuration.MessageJournal, configuration.AuthorizationService)},
                {"metrics", new MetricsController(_metricsCollector)},
            };
        }

        private async Task HandleMessage(Message message, CancellationToken cancellationToken)
        {
            var bus = await Bus;
            await bus.HandleMessage(message, Thread.CurrentPrincipal);
        }

        private static bool IsPlatibusUri(Uri uri, Uri baseUri)
        {
            var baseUriPath = baseUri.AbsolutePath.ToLower();
            var uriPath = uri.AbsolutePath.ToLower();
            return uriPath.StartsWith(baseUriPath);
        }

        /// <summary>
        /// Finalizer that ensures resources are released
        /// </summary>
        ~PlatibusMiddleware()
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
            if (!disposing) return;

            Bus.Dispose();
            _transportService.Dispose();
            _metricsCollector.Dispose();

            var disposableMessageQueueingService = _messageQueueingService as IDisposable;
            if (disposableMessageQueueingService != null)
            {
                disposableMessageQueueingService.Dispose();
            }

            var disposableMessageJournal = _messageJournal as IDisposable;
            if (disposableMessageJournal != null)
            {
                disposableMessageJournal.Dispose();
            }

            var disposableSubscriptionTrackingService = _subscriptionTrackingService as IDisposable;
            if (disposableSubscriptionTrackingService != null)
            {
                disposableSubscriptionTrackingService.Dispose();
            }
        }
    }
}
