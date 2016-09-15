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
using Common.Logging;
using Platibus.Http;

namespace Platibus.IIS
{
    /// <summary>
    /// HTTP handler used to handle requests in IIS
    /// </summary>
    public class PlatibusHttpHandler : HttpTaskAsyncHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(IISLoggingCategories.IIS);
        
        private readonly Task<IHttpResourceRouter> _resourceRouter;

        /// <summary>
        /// The base URI for requests handled by this handler
        /// </summary>
        public Uri BaseUri { get; private set; }

        /// <summary>
        /// When overridden in a derived class, gets a value that indicates whether the task handler class instance can be reused for another asynchronous task.
        /// </summary>
        /// <returns>
        /// true if the handler can be reused; otherwise, false.  The default is false.
        /// </returns>
        public override bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Inititializes a new <see cref="PlatibusHttpHandler"/> with the default
        /// configuration and configuration hooks
        /// </summary>
        public PlatibusHttpHandler()
        {
            _resourceRouter = InitResourceRouter();
        }

        /// <summary>
        /// Inititializes a new <see cref="PlatibusHttpHandler"/> with the configuration
        /// loaded from the named <paramref name="configurationSectionName">
        /// configuration section</paramref>
        /// </summary>
        /// <param name="configurationSectionName">The name of the configuration
        /// section from which the bus configuration should be loaded</param>
        public PlatibusHttpHandler(string configurationSectionName)
        {
            if (string.IsNullOrWhiteSpace(configurationSectionName)) throw new ArgumentNullException("configurationSectionName");
            _resourceRouter = InitResourceRouter(configurationSectionName);
        }

        /// <summary>
        /// Inititializes a new <see cref="PlatibusHttpHandler"/> with the specified
        /// <paramref name="bus"/> and <paramref name="configuration"/>
        /// </summary>
        /// <param name="bus">The initialized bus instance</param>
        /// <param name="configuration">The bus configuration</param>
        public PlatibusHttpHandler(Bus bus, IIISConfiguration configuration)
        {
            if (bus == null) throw new ArgumentNullException("bus");
            if (configuration == null) throw new ArgumentNullException("configuration");
            _resourceRouter = Task.FromResult(InitResourceRouter(bus, configuration));
        }
        
        private async Task<IHttpResourceRouter> InitResourceRouter()
        {
            var configuration = await IISConfigurationManager.LoadConfiguration();
            return await InitResourceRouter(configuration);
        }

        private async Task<IHttpResourceRouter> InitResourceRouter(string configSectionName)
        {
            var configuration = await IISConfigurationManager.LoadConfiguration(configSectionName);
            return await InitResourceRouter(configuration);
        }

        private async Task<IHttpResourceRouter> InitResourceRouter(IIISConfiguration configuration)
        {
            if (configuration == null)
            {
                configuration = await IISConfigurationManager.LoadConfiguration();
            }

            BaseUri = configuration.BaseUri;

            var managedBus = await BusManager.SingletonInstance.GetManagedBus(configuration);
            var bus = await managedBus.GetBus();
            return InitResourceRouter(bus, configuration);
        }

        private static IHttpResourceRouter InitResourceRouter(Bus bus, IIISConfiguration configuration)
        {
            var authorizationService = configuration.AuthorizationService;
            var subscriptionTrackingService = configuration.SubscriptionTrackingService;
            return new ResourceTypeDictionaryRouter
            {
                {"message", new MessageController(bus.HandleMessage, authorizationService)},
                {"topic", new TopicController(subscriptionTrackingService, configuration.Topics, authorizationService)}
            };
        }

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
            Log.DebugFormat("Processing {0} request for resource {1}...",
                context.Request.HttpMethod, context.Request.Url);

            var resourceRequest = new HttpRequestAdapter(context.Request, context.User);
            var resourceResponse = new HttpResponseAdapter(context.Response);
            try
            {
                var resourceRouter = await _resourceRouter;
                await resourceRouter.Route(resourceRequest, resourceResponse);

                Log.DebugFormat("{0} request for resource {1} processed successfully",
                    context.Request.HttpMethod, context.Request.Url);
            }
            catch (Exception ex)
            {
                var exceptionHandler = new HttpExceptionHandler(resourceRequest, resourceResponse, Log);
                exceptionHandler.HandleException(ex);
            }
        }
    }
}