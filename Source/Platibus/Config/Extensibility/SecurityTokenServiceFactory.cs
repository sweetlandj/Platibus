using System;
using System.Threading.Tasks;
using Platibus.Diagnostics;
using Platibus.Security;

namespace Platibus.Config.Extensibility
{
    /// <summary>
    /// Initializes a <see cref="ISecurityTokenService"/> based on the supplied configuration
    /// </summary>
    public class SecurityTokenServiceFactory
    {
        private readonly IDiagnosticService _diagnosticService;

        /// <summary>
        /// Initializes a new <see cref="SecurityTokenServiceFactory"/>
        /// </summary>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        public SecurityTokenServiceFactory(IDiagnosticService diagnosticService = null)
        {
            _diagnosticService = diagnosticService ?? DiagnosticService.DefaultInstance;
        }

        /// <summary>
        /// Initializes a security token service based on the supplied
        /// <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">The message queue configuration</param>
        /// <returns>Returns a task whose result is the initialized security token service</returns>
        public async Task<ISecurityTokenService> InitSecurityTokenService(SecurityTokensElement configuration)
        {
            var myConfig = configuration ?? new SecurityTokensElement();
            var provider = GetProvider(myConfig.Provider);
            if (string.IsNullOrWhiteSpace(myConfig.Provider))
            {
                await _diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                    {
                        Detail = "Message journal disabled"
                    }.Build());
                return null;
            }

            var messageJournal = await provider.CreateSecurityTokenService(myConfig);

            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.ComponentInitialization)
            {
                Detail = "Message journal initialized"
            }.Build());

            return messageJournal;
        }

        private ISecurityTokenServiceProvider GetProvider(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName)) throw new ArgumentNullException("providerName");
            return ProviderHelper.GetProvider<ISecurityTokenServiceProvider>(providerName);
        }
    }
}
