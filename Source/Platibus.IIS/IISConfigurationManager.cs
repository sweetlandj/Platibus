using System;
using System.Configuration;
using System.Threading.Tasks;
using Common.Logging;
using Platibus.Config;
using Platibus.Config.Extensibility;
using Platibus.Filesystem;

namespace Platibus.IIS
{
    public static class IISConfigurationManager
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Config);

        public static async Task<IISConfiguration> LoadConfiguration(string sectionName = "platibus.iis")
        {
            if (string.IsNullOrWhiteSpace(sectionName)) throw new ArgumentNullException("sectionName");
            var configSection = (IISConfigurationSection)ConfigurationManager.GetSection(sectionName) ?? new IISConfigurationSection();

            var configuration = await PlatibusConfigurationManager.LoadConfiguration<IISConfiguration>(sectionName);
            configuration.BaseUri = configSection.BaseUri;

            var subscriptionTracking = configSection.SubscriptionTracking ?? new SubscriptionTrackingElement();
            configuration.SubscriptionTrackingService = await InitSubscriptionTrackingService(subscriptionTracking);

            return configuration;
        }

        public static Task<ISubscriptionTrackingService> InitSubscriptionTrackingService(SubscriptionTrackingElement config)
        {
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
