using System;
using System.Configuration;
using System.Threading.Tasks;
using Common.Logging;
using Platibus.Config;
using Platibus.Config.Extensibility;
using Platibus.Filesystem;

namespace Platibus.IIS
{
    /// <summary>
    /// Factory class used to initialize <see cref="IISConfiguration"/> objects from
    /// declarative configuration elements in web configuration files.
    /// </summary>
    public static class IISConfigurationManager
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Config);

        /// <summary>
        /// Initializes and returns a <see cref="IISConfiguration"/> instance based on
        /// the <see cref="IISConfigurationSection"/> with the specified 
        /// <paramref name="sectionName"/>
        /// </summary>
        /// <param name="sectionName">(Optional) The name of the configuration section 
        /// (default is "platibus.iis")</param>
        /// <param name="processConfigurationHooks">(Optional) Whether to initialize and
        /// process implementations of <see cref="IConfigurationHook"/> found in the
        /// application domain (default is true)</param>
        /// <returns>Returns a task whose result will be an initialized 
        /// <see cref="PlatibusConfiguration"/> object</returns>
        /// <seealso cref="PlatibusConfigurationSection"/>
        /// <seealso cref="IConfigurationHook"/>
        public static async Task<IISConfiguration> LoadConfiguration(string sectionName = "platibus.iis",
            bool processConfigurationHooks = true)
        {
            if (string.IsNullOrWhiteSpace(sectionName)) throw new ArgumentNullException("sectionName");
            var configSection = (IISConfigurationSection) ConfigurationManager.GetSection(sectionName) ??
                                new IISConfigurationSection();

            var configuration = await PlatibusConfigurationManager.LoadConfiguration<IISConfiguration>(sectionName, processConfigurationHooks);
            configuration.BaseUri = configSection.BaseUri;
            configuration.BypassTransportLocalDestination = configSection.BypassTransportLocalDestination;

            var subscriptionTracking = configSection.SubscriptionTracking ?? new SubscriptionTrackingElement();
            configuration.SubscriptionTrackingService = await InitSubscriptionTrackingService(subscriptionTracking);
            configuration.MessageQueueingService = await PlatibusConfigurationManager.InitMessageQueueingService(configSection.Queueing);

            return configuration;
        }

        /// <summary>
        /// Helper method to initialize the subscription tracking service based on the
        /// supplied configuration element
        /// </summary>
        /// <param name="config">The subscription tracking configuration element</param>
        /// <returns>Returns a task whose result is an initialized subscription tracking service</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is
        /// <c>null</c></exception>
        public static Task<ISubscriptionTrackingService> InitSubscriptionTrackingService(
            SubscriptionTrackingElement config)
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