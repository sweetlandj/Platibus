using System.Threading.Tasks;
using Platibus.Diagnostics;
using Platibus.Filesystem;

namespace Platibus.Config.Extensibility
{
    /// <summary>
    /// Initializes a <see cref="IMessageQueueingService"/> based on the supplied configuration
    /// </summary>
    public class MessageQueueingServiceFactory
    {
        private readonly IDiagnosticService _diagnosticService;

        /// <summary>
        /// Initializes a new <see cref="MessageQueueingServiceFactory"/>
        /// </summary>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        public MessageQueueingServiceFactory(IDiagnosticService diagnosticService = null)
        {
            _diagnosticService = diagnosticService ?? DiagnosticService.DefaultInstance;
        }

        /// <summary>
        /// Initializes a message queueing service based on the supplied
        /// <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">The message queue configuration</param>
        /// <returns>Returns a task whose result is the initialized message queueing service</returns>
        public async Task<IMessageQueueingService> InitMessageQueueingService(QueueingElement configuration)
        {
            var myConfig = configuration ?? new QueueingElement();
            var provider = await GetProvider(myConfig.Provider);

            var messageQueueingService = await provider.CreateMessageQueueingService(myConfig);

            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.ComponentInitialization)
                {
                    Detail = "Message queueing service initialized"
                }.Build());

            return messageQueueingService;
        }

        private async Task<IMessageQueueingServiceProvider> GetProvider(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                await _diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                    {
                        Detail = "Using default filesystem-based message queueing service provider"
                    }.Build());

                return new FilesystemServicesProvider();
            }
            return ProviderHelper.GetProvider<IMessageQueueingServiceProvider>(providerName);
        }
    }
}
