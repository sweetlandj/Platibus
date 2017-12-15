using Microsoft.Extensions.DependencyInjection;
using Platibus.Http;
using Platibus.Http.Controllers;
using System.Threading.Tasks;

namespace Platibus.AspNetCore
{
    /// <summary>
    /// Registers Platibus services
    /// </summary>
    public static class PlatibusServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Platibus services to the supplied <paramref name="services"/> collection
        /// based on the configuration in appsettings.json
        /// </summary>
        /// <param name="services">The service collection to which the Platibus services
        /// will be added</param>
        /// <param name="sectionName">(Optional) The name of the Platibus configuration 
        /// section.  (The default section name is "platibus".)</param>
        public static void AddPlatibusServices(this IServiceCollection services, string sectionName = null)
        {
            var tcs = new TaskCompletionSource<bool>();
            services.AddPlatibusServicesAsync(sectionName)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted) tcs.TrySetException(t.Exception);
                    else if (t.IsCanceled) tcs.TrySetCanceled();
                    else tcs.TrySetResult(true);
                });
            tcs.Task.Wait();
        }

        /// <summary>
        /// Adds Platibus services to the supplied <paramref name="services"/> collection
        /// based on the specified <paramref name="configuration"/>
        /// </summary>
        /// <param name="services">The service collection to which the Platibus services
        /// will be added</param>
        /// <param name="configuration">The ASP.NET Core Platibus configuration</param>
        public static void AddPlatibusServices(this IServiceCollection services, IAspNetCoreConfiguration configuration)
        {
            var tcs = new TaskCompletionSource<bool>();
            services.AddPlatibusServicesAsync(configuration)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted) tcs.TrySetException(t.Exception);
                    else if (t.IsCanceled) tcs.TrySetCanceled();
                    else tcs.TrySetResult(true);
                });
            tcs.Task.Wait();
        }

        /// <summary>
        /// Adds Platibus services to the supplied <paramref name="services"/> collection
        /// based on the declarative configuration in appsettings.json.
        /// </summary>
        /// <param name="services">The service collection to which the Platibus services
        /// will be added</param>
        /// <param name="sectionName">(Optional) The name of the Platibus configuration 
        /// section.  (The default section name is "platibus".)</param>
        public static async Task AddPlatibusServicesAsync(this IServiceCollection services, string sectionName = null)
        {
            var configuration = new AspNetCoreConfiguration();
            await new AspNetCoreConfigurationManager().Initialize(configuration, sectionName);
            await services.AddPlatibusServicesAsync(configuration);
        }

        /// <summary>
        /// Adds Platibus services to the supplied <paramref name="services"/> collection
        /// based on the specified <paramref name="configuration"/>
        /// </summary>
        /// <param name="services">The service collection to which the Platibus services
        /// will be added</param>
        /// <param name="configuration">The ASP.NET Core Platibus configuration</param>
        public static async Task AddPlatibusServicesAsync(this IServiceCollection services,
        IAspNetCoreConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddSingleton(configuration.DiagnosticService);
            services.AddSingleton(configuration.MessageQueueingService);
            services.AddSingleton(configuration.SubscriptionTrackingService);
            if (configuration.MessageJournal != null)
            {
                services.AddSingleton(configuration.MessageJournal);
            }

            var metricsCollector = new HttpMetricsCollector();
            services.AddSingleton(metricsCollector);
            configuration.DiagnosticService.AddSink(metricsCollector);

            var baseUri = configuration.BaseUri;
            var endpoints = configuration.Endpoints;
            var transportService = new HttpTransportService(baseUri, endpoints,
                configuration.MessageQueueingService,
                configuration.MessageJournal,
                configuration.SubscriptionTrackingService,
                configuration.BypassTransportLocalDestination,
                configuration.DiagnosticService);

            services.AddSingleton(transportService);

            var bus = new Bus(configuration, baseUri, transportService, configuration.MessageQueueingService);
            services.AddSingleton<IBus>(bus);

            transportService.LocalDelivery += (sender, args) => bus.HandleMessage(args.Message, args.Principal);

            var authorizationService = configuration.AuthorizationService;
            var subscriptionTrackingService = configuration.SubscriptionTrackingService;
            var resourceRouter = new ResourceTypeDictionaryRouter(configuration.BaseUri)
            {
                {"message", new MessageController(bus.HandleMessage, authorizationService)},
                {"topic", new TopicController(subscriptionTrackingService, configuration.Topics, authorizationService)},
                {"journal", new JournalController(configuration.MessageJournal, configuration.AuthorizationService)},
                {"metrics", new MetricsController(metricsCollector)},
            };

            services.AddSingleton<IHttpResourceRouter>(resourceRouter);

            await transportService.Init();
            await bus.Init();
        }
    }
}
