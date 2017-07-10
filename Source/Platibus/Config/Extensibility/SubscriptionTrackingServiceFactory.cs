using System.Threading.Tasks;
using Platibus.Diagnostics;
using Platibus.Filesystem;

namespace Platibus.Config.Extensibility
{
    /// <summary>
    /// Initializes a <see cref="ISubscriptionTrackingService"/> based on the supplied configuration
    /// </summary>
    public class SubscriptionTrackingServiceFactory
    {
        private readonly IDiagnosticService _diagnosticService;

        /// <summary>
        /// Initializes a new <see cref="SubscriptionTrackingServiceFactory"/>
        /// </summary>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        public SubscriptionTrackingServiceFactory(IDiagnosticService diagnosticService = null)
        {
            _diagnosticService = diagnosticService ?? DiagnosticService.DefaultInstance;
        }

        /// <summary>
        /// Initializes a subscription tracking service based on the supplied
        /// <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">The subscription tracking configuration</param>
        /// <returns>Returns a task whose result is the initialized subscription tracking service</returns>
        public async Task<ISubscriptionTrackingService> InitSubscriptionTrackingService(SubscriptionTrackingElement configuration)
        {
            var myConfig = configuration ?? new SubscriptionTrackingElement();
            var provider = await GetProvider(myConfig.Provider);

            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.ComponentInitialization)
                {
                    Detail = "Initializing subscription tracking service"
                }.Build());

            var messageQueueingService = await provider.CreateSubscriptionTrackingService(myConfig);

            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.ComponentInitialization)
                {
                    Detail = "Subscription tracking service initialized"
                }.Build());

            return messageQueueingService;
        }

        private async Task<ISubscriptionTrackingServiceProvider> GetProvider(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                await _diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                    {
                        Detail = "Using default filesystem-based subscription tracking service provider"
                    }.Build());

                return new FilesystemServicesProvider();
            }
            return ProviderHelper.GetProvider<ISubscriptionTrackingServiceProvider>(providerName);
        }
    }
}