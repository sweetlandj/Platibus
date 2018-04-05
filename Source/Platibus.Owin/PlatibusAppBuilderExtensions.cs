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
using Microsoft.Owin.BuilderProperties;
using Owin;
using Platibus.Http;
using Platibus.Utils;

namespace Platibus.Owin
{
    public static class PlatibusAppBuilderExtensions
    {
        public static IAppBuilder UsePlatibusMiddleware(this IAppBuilder app, string sectionName)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            return app.UsePlatibusMiddlewareAsync(sectionName, null).GetResultUsingContinuation();
        }

        public static IAppBuilder UsePlatibusMiddleware(this IAppBuilder app, IOwinConfiguration configuration)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            return app.UsePlatibusMiddlewareAsync(configuration).GetResultUsingContinuation();
        }

        public static IAppBuilder UsePlatibusMiddleware(this IAppBuilder app, Task<IOwinConfiguration> configuration)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            var configurationResult = configuration.GetResultUsingContinuation();
            return app.UsePlatibusMiddlewareAsync(configurationResult).GetResultUsingContinuation();
        }

        public static IAppBuilder UsePlatibusMiddleware(this IAppBuilder app, PlatibusMiddleware middleware, bool disposeMiddleware = false)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (middleware == null) throw new ArgumentNullException(nameof(app));

            if (disposeMiddleware)
            {
                var appProperties = new AppProperties(app.Properties);
                appProperties.OnAppDisposing.Register(middleware.Dispose);
            }

            return app.Use(middleware.Invoke);
        }

        public static IAppBuilder UsePlatibusMiddleware(this IAppBuilder app)
        {
            return app.UsePlatibusMiddlewareAsync(null, null).GetResultUsingContinuation();
        }

        public static IAppBuilder UsePlatibusMiddleware(this IAppBuilder app, Action<OwinConfiguration> configure)
        {
            return app.UsePlatibusMiddlewareAsync(null, configuration =>
            {
                configure?.Invoke(configuration);
                return Task.FromResult(0);
            }).GetResultUsingContinuation();
        }
        
        public static IAppBuilder UsePlatibusMiddleware(this IAppBuilder app, Func<OwinConfiguration, Task> configure)
        {
            return app.UsePlatibusMiddlewareAsync(null, configure).GetResultUsingContinuation(); 
        }

        public static IAppBuilder UsePlatibusMiddleware(this IAppBuilder app, string sectionName, Action<OwinConfiguration> configure)
        {
            return app.UsePlatibusMiddlewareAsync(sectionName, cfg =>
            {
                configure?.Invoke(cfg);
                return Task.FromResult(0);
            }).GetResultUsingContinuation();
        }

        public static IAppBuilder UsePlatibusMiddleware(this IAppBuilder app, string sectionName, Func<OwinConfiguration, Task> configure)
        {
            return app.UsePlatibusMiddlewareAsync(sectionName, configure).GetResultUsingContinuation();
        }

        private static async Task<IAppBuilder> UsePlatibusMiddlewareAsync(this IAppBuilder app,
            string sectionName,
            Func<OwinConfiguration, Task> configure)
        {
            var configuration = new OwinConfiguration();
            var configurationManager = new OwinConfigurationManager();
            await configurationManager.Initialize(configuration, sectionName);
            await configurationManager.FindAndProcessConfigurationHooks(configuration);
            if (configure != null)
            {
                await configure(configuration);
            }

            return await app.UsePlatibusMiddlewareAsync(configuration);
        }

        private static async Task<IAppBuilder> UsePlatibusMiddlewareAsync(this IAppBuilder app,
            IOwinConfiguration configuration)
        {
            var appProperties = new AppProperties(app.Properties);

            var baseUri = configuration.BaseUri;
            var subscriptionTrackingService = configuration.SubscriptionTrackingService;
            var messageQueueingService = configuration.MessageQueueingService;
            
            var transportServiceOptions = new HttpTransportServiceOptions(baseUri, messageQueueingService, subscriptionTrackingService)
            {
                DiagnosticService = configuration.DiagnosticService,
                Endpoints = configuration.Endpoints,
                MessageJournal = configuration.MessageJournal,
                BypassTransportLocalDestination = configuration.BypassTransportLocalDestination
            };

            var transportService = new HttpTransportService(transportServiceOptions);
            appProperties.OnAppDisposing.Register(transportService.Dispose);

            var bus = new Bus(configuration, baseUri, transportService, messageQueueingService);
            transportService.LocalDelivery += (sender, args) => bus.HandleMessage(args.Message, args.Principal);

            await transportService.Init();
            await bus.Init();

            var middleware = new PlatibusMiddleware(configuration, bus);

            appProperties.OnAppDisposing.Register(() =>
            {
                middleware.Dispose();
                transportService.Dispose();
                bus.Dispose();
                (messageQueueingService as IDisposable)?.Dispose();
                (subscriptionTrackingService as IDisposable)?.Dispose();
            });

            return app.Use(middleware.Invoke);
        }
    }
}
