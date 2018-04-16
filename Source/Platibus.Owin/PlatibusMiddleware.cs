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

using Microsoft.Owin;
using Platibus.Diagnostics;
using Platibus.Http;
using Platibus.Http.Controllers;
using System;
using System.Threading.Tasks;

namespace Platibus.Owin
{
    public class PlatibusMiddleware : IDisposable
    {
        private readonly HttpMetricsCollector _metricsCollector = new HttpMetricsCollector();
        private readonly ResourceTypeDictionaryRouter _resourceRouter;

        private bool _disposed;

        public IOwinConfiguration Configuration { get; }
        public IBus Bus { get; }

        public PlatibusMiddleware(IOwinConfiguration configuration, IBus bus, HttpTransportService transportService)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(bus));
            Bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _resourceRouter = InitResourceRouter(Configuration, transportService);
        }

        public async Task Invoke(IOwinContext context, Func<Task> next)
        {
            context.SetBus(Bus);

            var handled = await HandlePlatibusRequest(context, Configuration.DiagnosticService);
            if (!handled && next != null)
            {
                await next();
            }
        }

        private async Task<bool> HandlePlatibusRequest(IOwinContext context, IDiagnosticService diagnosticService)
        {
            if (!_resourceRouter.IsRoutable(context.Request.Uri)) return false;
            
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
                await _resourceRouter.Route(resourceRequest, resourceResponse);
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

            return true;
        }

        private ResourceTypeDictionaryRouter InitResourceRouter(IOwinConfiguration configuration, HttpTransportService transportService)
        {
            var authorizationService = configuration.AuthorizationService;
            var subscriptionTrackingService = configuration.SubscriptionTrackingService;
            return new ResourceTypeDictionaryRouter(configuration.BaseUri)
            {
                {"message", new MessageController(transportService.ReceiveMessage, authorizationService)},
                {"topic", new TopicController(subscriptionTrackingService, configuration.Topics, authorizationService)},
                {"journal", new JournalController(configuration.MessageJournal, configuration.AuthorizationService)},
                {"metrics", new MetricsController(_metricsCollector)},
            };
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

            _metricsCollector.Dispose();
        }
    }
}
