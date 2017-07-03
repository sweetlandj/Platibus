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
        private readonly IDiagnosticEventSink _diagnosticEventSink;

        /// <summary>
        /// Initializes a new <see cref="SecurityTokenServiceFactory"/>
        /// </summary>
        /// <param name="diagnosticEventSink">(Optional) The event sink to which diagnostic events 
        /// related to security token service initialization will be sent</param>
        public SecurityTokenServiceFactory(IDiagnosticEventSink diagnosticEventSink = null)
        {
            _diagnosticEventSink = diagnosticEventSink ?? NoopDiagnosticEventSink.Instance;
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
                await _diagnosticEventSink.ReceiveAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                    {
                        Detail = "Message journal disabled"
                    }.Build());
                return null;
            }

            var messageJournal = await provider.CreateSecurityTokenService(myConfig);

            await _diagnosticEventSink.ReceiveAsync(
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
