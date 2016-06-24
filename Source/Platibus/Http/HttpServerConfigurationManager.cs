using System;
using System.Configuration;
using System.Threading.Tasks;
using Common.Logging;
using Platibus.Config;
using Platibus.Config.Extensibility;
using Platibus.Filesystem;

namespace Platibus.Http
{
    /// <summary>
    /// Helper class for loading HTTP server configuration information
    /// </summary>
    public static class HttpServerConfigurationManager
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Config);

        /// <summary>
        /// Initializes an <see cref="HttpServerConfiguration"/> object based on the data in the named
        /// <see cref="HttpServerConfigurationSection"/>
        /// </summary>
        /// <param name="sectionName">(Optional) The name of the configuration section</param>
        /// <returns>Returns a task that will complete when the HTTP server configuration has been 
        /// loaded and initialized and whose result will be the initialized configuration</returns>
        public static async Task<HttpServerConfiguration> LoadConfiguration(string sectionName = "platibus.httpserver")
        {
            if (string.IsNullOrWhiteSpace(sectionName)) throw new ArgumentNullException("sectionName");
            var configSection = (HttpServerConfigurationSection) ConfigurationManager.GetSection(sectionName) ??
                                new HttpServerConfigurationSection();

            var configuration =
                await PlatibusConfigurationManager.LoadConfiguration<HttpServerConfiguration>(sectionName);
            configuration.BaseUri = configSection.BaseUri;
            configuration.ConcurrencyLimit = configSection.ConcurrencyLimit;
            configuration.AuthenticationSchemes = configSection.AuthenticationSchemes.GetFlags();
            configuration.BypassTransportLocalDestination = configSection.BypassTransportLocalDestination;

            var subscriptionTracking = configSection.SubscriptionTracking ?? new SubscriptionTrackingElement();
            configuration.SubscriptionTrackingService = await InitSubscriptionTrackingService(subscriptionTracking);
            configuration.MessageQueueingService = await PlatibusConfigurationManager.InitMessageQueueingService(configSection.Queueing);

            return configuration;
        }

        /// <summary>
        /// Helper method used to initialize an <see cref="ISubscriptionTrackingService"/> based on
        /// the configuration in a <see cref="SubscriptionTrackingElement"/>
        /// </summary>
        /// <param name="config">The configuration element</param>
        /// <returns>Returns a task that will complete when the subscription tracking service has
        /// initialized and whose result is the initialized subscription tracking service</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is <c>null</c>
        /// </exception>
        public static Task<ISubscriptionTrackingService> InitSubscriptionTrackingService(
            SubscriptionTrackingElement config)
        {
            if (config == null) throw new ArgumentNullException("config");
            var providerName = config.Provider;
            ISubscriptionTrackingServiceProvider provider;
            if (string.IsNullOrWhiteSpace(providerName))
            {
                Log.Debug("No subscription tracking service provider specified; using default provider...");
                provider = new FilesystemServicesProvider();
            }
            else
            {
                provider = ProviderHelper.GetProvider<ISubscriptionTrackingServiceProvider>(providerName);
            }

            Log.Debug("Initializing subscription tracking service...");
            return provider.CreateSubscriptionTrackingService(config);
        }
    }
}