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
using System.Threading.Tasks;
using System.Web;
using Platibus.Diagnostics;
using Platibus.Http;
using Platibus.Http.Controllers;
using Platibus.Utils;

namespace Platibus.IIS
{
    /// <inheritdoc />
    /// <summary>
    /// HTTP handler used to handle requests in IIS
    /// </summary>
    public class PlatibusHttpHandler : HttpTaskAsyncHandler
    {
        /// <summary>
        /// Singleton metrics collector
        /// </summary>
        /// <remarks>
        /// An HTTP handler is created for each request and possibly pooled depending on the value
        /// returned by <see cref="IsReusable"/>.  Because of this it is possible for several
        /// HTTP handlers to exist at the same time.  In order to aggregate metrics across all
        /// of these handlers, a static singleton metrics collector is needed.
        /// </remarks>
        private static readonly HttpMetricsCollector SingletonMetricsCollector = new HttpMetricsCollector();

        
        static PlatibusHttpHandler()
        {
            DiagnosticService.DefaultInstance.AddSink(SingletonMetricsCollector);
        }

        private readonly IIISConfiguration _configuration;
        private readonly IHttpResourceRouter _resourceRouter;

        public IBus Bus { get; }

        /// <summary>
        /// The base URI for requests handled by this handler
        /// </summary>
        public Uri BaseUri { get; }

        /// <inheritdoc />
        /// <summary>
        /// When overridden in a derived class, gets a value that indicates whether the task handler class instance can be reused for another asynchronous task.
        /// </summary>
        /// <returns>
        /// true if the handler can be reused; otherwise, false.  The default is false.
        /// </returns>
        public override bool IsReusable => true;

        /// <inheritdoc />
        /// <summary>
        /// Inititializes a new <see cref="T:Platibus.IIS.PlatibusHttpHandler" /> with the default
        /// configuration and configuration hooks
        /// </summary>
        public PlatibusHttpHandler()
            : this(IISConfigurationCache.SingletonInstance.GetConfiguration(null))
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Inititializes a new <see cref="T:Platibus.IIS.PlatibusHttpHandler" /> with the default
        /// configuration and configuration hooks
        /// </summary>
        public PlatibusHttpHandler(string configSectionName, Action<IISConfiguration> configure)
            : this(IISConfigurationCache.SingletonInstance.GetConfiguration(configSectionName, configure))
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Inititializes a new <see cref="T:Platibus.IIS.PlatibusHttpHandler" /> with the default
        /// configuration and configuration hooks
        /// </summary>
        public PlatibusHttpHandler(string configSectionName, Func<IISConfiguration, Task> configure)
            : this(IISConfigurationCache.SingletonInstance.GetConfiguration(configSectionName, configure))
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Inititializes a new <see cref="T:Platibus.IIS.PlatibusHttpHandler" /> with the configuration
        /// loaded from the named <paramref name="configurationSectionName">
        /// configuration section</paramref>
        /// </summary>
        /// <param name="configurationSectionName">The name of the configuration
        /// section from which the bus configuration should be loaded</param>
        public PlatibusHttpHandler(string configurationSectionName = null)
            : this(IISConfigurationCache.SingletonInstance.GetConfiguration(configurationSectionName))
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.IIS.PlatibusHttpHandler" /> with the configuration that will
        /// eventually be returned by the supplied <paramref name="configuration" /> task.
        /// </summary>
        /// <param name="configuration">A task whose result is the configuration to use for this
        /// handler</param>
        public PlatibusHttpHandler(Task<IIISConfiguration> configuration)
            : this(configuration?.GetResultFromCompletionSource())
        {
            _configuration = configuration?.GetResultFromCompletionSource() ?? throw new ArgumentNullException(nameof(configuration));
            Bus = BusManager.SingletonInstance.GetBus(_configuration);
            _resourceRouter = InitResourceRouter(Bus, _configuration);
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.IIS.PlatibusHttpHandler" /> with the configuration that will
        /// eventually be returned by the supplied <paramref name="configuration" /> task.
        /// </summary>
        /// <param name="configuration">A task whose result is the configuration to use for this
        /// handler</param>
        public PlatibusHttpHandler(IIISConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Bus = BusManager.SingletonInstance.GetBus(_configuration);
            BaseUri = _configuration.BaseUri;
            _resourceRouter = InitResourceRouter(Bus, _configuration);
        }

        /// <inheritdoc />
        /// <summary>
        /// Inititializes a new <see cref="T:Platibus.IIS.PlatibusHttpHandler" /> with the specified
        /// <paramref name="bus" /> and <paramref name="configuration" />
        /// </summary>
        /// <param name="bus">The initialized bus instance</param>
        /// <param name="configuration">The bus configuration</param>
        /// <remarks>
        /// Used internally by <see cref="T:Platibus.IIS.PlatibusHttpModule" />.  This method bypasses the
        /// configuration cache and singleton diagnostic service and metrics collector. 
        /// </remarks>
        internal PlatibusHttpHandler(IBus bus, IIISConfiguration configuration)
        {
            Bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            BaseUri = configuration.BaseUri;
           _resourceRouter = InitResourceRouter(bus, configuration);
        }

        

        private static IHttpResourceRouter InitResourceRouter(IBus bus, IIISConfiguration configuration)
        {
            var authorizationService = configuration.AuthorizationService;
            var subscriptionTrackingService = configuration.SubscriptionTrackingService;
            return new ResourceTypeDictionaryRouter(configuration.BaseUri)
            {
                {"message", new MessageController(bus.HandleMessage, authorizationService)},
                {"topic", new TopicController(subscriptionTrackingService, configuration.Topics, authorizationService)},
                {"journal", new JournalController(configuration.MessageJournal, configuration.AuthorizationService)},
                {"metrics", new MetricsController(SingletonMetricsCollector)}
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// When overridden in a derived class, provides code that handles an asynchronous task.
        /// </summary>
        /// <returns>
        /// The asynchronous task.
        /// </returns>
        /// <param name="context">The HTTP context.</param>
        public override Task ProcessRequestAsync(HttpContext context)
        {
            return ProcessRequestAsync(new HttpContextWrapper(context));
        }

        private async Task ProcessRequestAsync(HttpContextBase context)
        {
            var diagnosticService = _configuration.DiagnosticService;

            await diagnosticService.EmitAsync(
                new HttpEventBuilder(this, HttpEventType.HttpRequestReceived)
                {
                    Remote = context.Request.UserHostAddress,
                    Uri = context.Request.Url,
                    Method = context.Request.HttpMethod
                }.Build());
            
            var resourceRequest = new HttpRequestAdapter(context.Request, context.User);
            var resourceResponse = new HttpResponseAdapter(context.Response);
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
                new HttpEventBuilder(this, HttpEventType.HttpRequestReceived)
                {
                    Remote = context.Request.UserHostAddress,
                    Uri = context.Request.Url,
                    Method = context.Request.HttpMethod,
                    Status = context.Response.StatusCode
                }.Build());
        }
    }
}