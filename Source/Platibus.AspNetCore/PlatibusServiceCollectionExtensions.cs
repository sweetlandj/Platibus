using System;
using Microsoft.Extensions.DependencyInjection;
using Platibus.Http;
using Platibus.Http.Controllers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platibus.Diagnostics;
using Platibus.Journaling;
using Platibus.Security;
using Platibus.Utils;

namespace Platibus.AspNetCore
{
    /// <summary>
    /// Registers Platibus services
    /// </summary>
    public static class PlatibusServiceCollectionExtensions
    {
        /// <summary>
        /// Initializes an <see cref="AspNetCoreConfiguration"/> based on default values and
        /// overrides specified in the default configuration section name (if present)
        /// </summary>
        /// <param name="services">The service collection to which the configuration will be
        /// added</param>
        public static void ConfigurePlatibus(this IServiceCollection services)
        {
            services.ConfigurePlatibus(null, null);
        }

        /// <summary>
        /// Adds the specified <paramref name="configuration"/> to the service collection
        /// </summary>
        /// <param name="services">The service collection to which the configuration will be
        /// added</param>
        /// <param name="configuration">The configuration to add to the service collection</param>
        public static void ConfigurePlatibus(this IServiceCollection services, IAspNetCoreConfiguration configuration)
        {
            services.AddSingleton(configuration ?? new AspNetCoreConfiguration());
        }

        /// <summary>
        /// Initializes an <see cref="AspNetCoreConfiguration"/> based on default values and
        /// overrides specified in the supplied <paramref name="sectionName"/>
        /// </summary>
        /// <param name="services">The service collection to which the configuration will be
        /// added</param>
        /// <param name="sectionName">The name of the configuration section from which the
        /// ASP.NET core configuration should be initialized</param>
        public static void ConfigurePlatibus(this IServiceCollection services, string sectionName)
        {
            services.ConfigurePlatibus(sectionName, null);
        }
        
        /// <summary>
        /// Initializes an <see cref="AspNetCoreConfiguration"/> based on default values,
        /// overrides specified in the default configuration section name, and custom
        /// configuration applied by the <paramref name="configure"/> callback.
        /// </summary>
        /// <param name="services">The service collection to which the configuration will be
        /// added</param>
        /// <param name="configure">A callback used to apply customizations to the
        /// initialized configuration object</param>
        public static void ConfigurePlatibus(this IServiceCollection services,
            Action<AspNetCoreConfiguration> configure)
        {
            services.ConfigurePlatibus(null, configuration =>
            {
                configure?.Invoke(configuration);
                return Task.FromResult(0);
            });
        }

        /// <summary>
        /// Initializes an <see cref="AspNetCoreConfiguration"/> based on default values,
        /// overrides specified in the default configuration section name, and custom
        /// configuration applied by the <paramref name="configure"/> callback.
        /// </summary>
        /// <param name="services">The service collection to which the configuration will be
        /// added</param>
        /// <param name="configure">A callback used to apply customizations to the
        /// initialized configuration object</param>
        public static void ConfigurePlatibus(this IServiceCollection services,
            Func<AspNetCoreConfiguration, Task> configure)
        {
            services.ConfigurePlatibus(null, configuration =>
            {
                configure?.Invoke(configuration).WaitOnCompletionSource();
            });
        }

        /// <summary>
        /// Initializes an <see cref="AspNetCoreConfiguration"/> based on default values,
        /// overrides specified in the supplied <paramref name="sectionName"/>, and custom
        /// configuration applied by the <paramref name="configure"/> callback.
        /// </summary>
        /// <param name="services">The service collection to which the configuration will be
        /// added</param>
        /// <param name="sectionName">The name of the configuration section from which the
        /// ASP.NET core configuration should be initialized</param>
        /// <param name="configure">A callback used to apply customizations to the
        /// initialized configuration object</param>
        public static void ConfigurePlatibus(this IServiceCollection services,
            string sectionName,
            Action<AspNetCoreConfiguration> configure)
        {
            services.ConfigurePlatibus(sectionName, configuration =>
            {
                configure?.Invoke(configuration);
                return Task.FromResult(0);
            });
        }

        /// <summary>
        /// Initializes an <see cref="AspNetCoreConfiguration"/> based on default values,
        /// overrides specified in the supplied <paramref name="sectionName"/>, and custom
        /// configuration applied by the <paramref name="configure"/> callback.
        /// </summary>
        /// <param name="services">The service collection to which the configuration will be
        /// added</param>
        /// <param name="sectionName">The name of the configuration section from which the
        /// ASP.NET core configuration should be initialized</param>
        /// <param name="configure">A callback used to apply customizations to the
        /// initialized configuration object</param>
        public static void ConfigurePlatibus(this IServiceCollection services,
            string sectionName,
            Func<AspNetCoreConfiguration, Task> configure)
        {
            services.AddSingleton(provider => InitializePlatibusConfiguration(sectionName, configure).GetResultFromCompletionSource());
        }

        /// <summary>
        /// Adds Platibus services to the specified service collection
        /// </summary>
        /// <param name="services">The service collection to which the Platibus services will
        /// be added</param>
        public static void AddPlatibus(this IServiceCollection services)
        {
            services.TryAdd(ServiceDescriptor.Singleton(provider =>
                InitializePlatibusConfiguration(null, null).GetResultFromCompletionSource()));

            services.AddSingleton<AspNetCoreLoggingSink>();

            services.AddSingleton(serviceProvider => serviceProvider.GetService<IAspNetCoreConfiguration>().DiagnosticService);
            services.AddSingleton(serviceProvider => serviceProvider.GetService<IAspNetCoreConfiguration>().MessageQueueingService);
            services.AddSingleton(serviceProvider => serviceProvider.GetService<IAspNetCoreConfiguration>().SubscriptionTrackingService);
            services.AddSingleton(serviceProvider => serviceProvider.GetService<IAspNetCoreConfiguration>().MessageJournal);
            services.AddSingleton(serviceProvider => serviceProvider.GetService<IAspNetCoreConfiguration>().AuthorizationService);
            
            services.AddSingleton(serviceProvider =>
            {
                var diagnosticsService = serviceProvider.GetService<IDiagnosticService>();

                var metricsCollector = new HttpMetricsCollector();
                diagnosticsService.AddSink(metricsCollector);
                return metricsCollector;
            });

            services.AddSingleton<ITransportService>(serviceProvider =>
            {
                var configuration = serviceProvider.GetService<IAspNetCoreConfiguration>();
                var diagnosticsService = serviceProvider.GetService<IDiagnosticService>();
                var messageQueueingService = serviceProvider.GetService<IMessageQueueingService>();
                var subscriptionTrackingService = serviceProvider.GetService<ISubscriptionTrackingService>();
                var messageJournal = serviceProvider.GetService<IMessageJournal>();

                var transportServiceOptions = new HttpTransportServiceOptions(configuration.BaseUri, messageQueueingService, subscriptionTrackingService)
                {
                    DiagnosticService = diagnosticsService,
                    Endpoints = configuration.Endpoints,
                    MessageJournal = messageJournal,
                    HttpClientFactory = configuration.HttpClientFactory,
                    BypassTransportLocalDestination = configuration.BypassTransportLocalDestination
                };
                var transportService = new HttpTransportService(transportServiceOptions);
                transportService.Init().WaitOnCompletionSource();
                return transportService;
            });

            services.AddSingleton<IHttpResourceRouter>(serviceProvider =>
            {
                var configuration = serviceProvider.GetService<IAspNetCoreConfiguration>();
                var transportService = serviceProvider.GetService<ITransportService>();
                var subscriptionTrackingService = serviceProvider.GetService<ISubscriptionTrackingService>();
                var messageJournal = serviceProvider.GetService<IMessageJournal>();
                var metricsCollector = serviceProvider.GetService<HttpMetricsCollector>();
                var authorizationService = serviceProvider.GetService<IAuthorizationService>();

                return new ResourceTypeDictionaryRouter(configuration.BaseUri)
                {
                    {"message", new MessageController(transportService.ReceiveMessage, authorizationService)},
                    {"topic", new TopicController(subscriptionTrackingService, configuration.Topics, authorizationService)},
                    {"journal", new JournalController(messageJournal, authorizationService)},
                    {"metrics", new MetricsController(metricsCollector)}
                };
            });

            services.AddSingleton<IBus>(serviceProvider =>
            {
                var configuration = serviceProvider.GetService<IAspNetCoreConfiguration>();
                var transportService = serviceProvider.GetService<ITransportService>();
                var messageQueueingService = serviceProvider.GetService<IMessageQueueingService>();

                var bus = new Bus(configuration, configuration.BaseUri, transportService, messageQueueingService);
                bus.Init().WaitOnCompletionSource();
                return bus;
            });

        }

        private static async Task<IAspNetCoreConfiguration> InitializePlatibusConfiguration(string sectionName, Func<AspNetCoreConfiguration, Task> configure)
        {
            var configuration = new AspNetCoreConfiguration();
            var configurationManager = new AspNetCoreConfigurationManager();
            await configurationManager.Initialize(configuration, sectionName);
#pragma warning disable 612
            await configurationManager.FindAndProcessConfigurationHooks(configuration);
#pragma warning restore 612
            if (configure != null)
            {
                await configure(configuration);
            }
            return configuration;
        }

        /// <summary>
        /// Adds Platibus services to the supplied <paramref name="services"/> collection
        /// based on the configuration in appsettings.json
        /// </summary>
        /// <param name="services">The service collection to which the Platibus services
        /// will be added</param>
        [Obsolete("Use ConfigurePlatibus and AddPlatibus")]
        public static void AddPlatibusServices(this IServiceCollection services)
        {
            services.ConfigurePlatibus();
            services.AddPlatibus();
        }

        /// <summary>
        /// Adds Platibus services to the supplied <paramref name="services"/> collection
        /// based on the configuration in appsettings.json
        /// </summary>
        /// <param name="services">The service collection to which the Platibus services
        /// will be added</param>
        /// <param name="sectionName">(Optional) The name of the Platibus configuration 
        /// section.  (The default section name is "platibus".)</param>
        [Obsolete("Use ConfigurePlatibus and AddPlatibus")]
        public static void AddPlatibusServices(this IServiceCollection services, string sectionName)
        {
            services.ConfigurePlatibus(sectionName);
            services.AddPlatibus();
        }

        /// <summary>
        /// Adds Platibus services to the supplied <paramref name="services"/> collection
        /// based on the configuration in appsettings.json
        /// </summary>
        /// <param name="services">The service collection to which the Platibus services
        /// will be added</param>
        /// <param name="configure">(Optional) Additional configuration to be performed
        /// after the configuration is loaded from appsettings.json</param>
        [Obsolete("Use ConfigurePlatibus and AddPlatibus")]
        public static void AddPlatibusServices(this IServiceCollection services, Action<AspNetCoreConfiguration> configure)
        {
            services.ConfigurePlatibus(configure);
            services.AddPlatibus();
        }

        /// <summary>
        /// Adds Platibus services to the supplied <paramref name="services"/> collection
        /// based on the configuration in appsettings.json
        /// </summary>
        /// <param name="services">The service collection to which the Platibus services
        /// will be added</param>
        /// <param name="sectionName">(Optional) The name of the Platibus configuration 
        /// section.  (The default section name is "platibus".)</param>
        /// <param name="configure">(Optional) Additional configuration to be performed
        /// after the configuration is loaded from appsettings.json</param>
        [Obsolete("Use ConfigurePlatibus and AddPlatibus")]
        public static void AddPlatibusServices(this IServiceCollection services, string sectionName, Action<AspNetCoreConfiguration> configure)
        {
            services.ConfigurePlatibus(sectionName, configure);
            services.AddPlatibus();
        }

        /// <summary>
        /// Adds Platibus services to the supplied <paramref name="services"/> collection
        /// based on the configuration in appsettings.json
        /// </summary>
        /// <param name="services">The service collection to which the Platibus services
        /// will be added</param>
        /// <param name="sectionName">(Optional) The name of the Platibus configuration 
        /// section.  (The default section name is "platibus".)</param>
        /// <param name="configure">(Optional) Additional configuration to be performed
        /// after the configuration is loaded from appsettings.json</param>
        [Obsolete("Use ConfigurePlatibus and AddPlatibus")]
        public static void AddPlatibusServices(this IServiceCollection services, string sectionName, Func<AspNetCoreConfiguration, Task> configure)
        {
            services.ConfigurePlatibus(sectionName, configure);
            services.AddPlatibus();
        }

        /// <summary>
        /// Adds Platibus services to the supplied <paramref name="services"/> collection
        /// based on the specified <paramref name="configuration"/>
        /// </summary>
        /// <param name="services">The service collection to which the Platibus services
        /// will be added</param>
        /// <param name="configuration">The ASP.NET Core Platibus configuration</param>
        [Obsolete("Use ConfigurePlatibus and AddPlatibus")]
        public static void AddPlatibusServices(this IServiceCollection services, IAspNetCoreConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddPlatibus();
        }

        /// <summary>
        /// Adds Platibus services to the supplied <paramref name="services"/> collection
        /// based on the declarative configuration in appsettings.json.
        /// </summary>
        /// <param name="services">The service collection to which the Platibus services
        /// will be added</param>
        /// <param name="sectionName">(Optional) The name of the Platibus configuration 
        /// section.  (The default section name is "platibus".)</param>
        /// <param name="configure">(Optional) Additional configuration to be performed
        /// after the configuration is loaded from appsettings.json</param>
        [Obsolete("Use ConfigurePlatibus and AddPlatibus")]
        public static Task AddPlatibusServicesAsync(this IServiceCollection services, string sectionName = null, Func<AspNetCoreConfiguration, Task> configure = null)
        {
            services.ConfigurePlatibus(sectionName, configure);
            services.AddPlatibus();
            return Task.FromResult(0);
        }
        
        /// <summary>
        /// Adds Platibus services to the supplied <paramref name="services"/> collection
        /// based on the specified <paramref name="configuration"/>
        /// </summary>
        /// <param name="services">The service collection to which the Platibus services
        /// will be added</param>
        /// <param name="configuration">The ASP.NET Core Platibus configuration</param>
        [Obsolete("Use ConfigurePlatibus and AddPlatibus")]
        public static Task AddPlatibusServicesAsync(this IServiceCollection services,
            IAspNetCoreConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddPlatibus();
            return Task.FromResult(0);
        }
        
    }
}
