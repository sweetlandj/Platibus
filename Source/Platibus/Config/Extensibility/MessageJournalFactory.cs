using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Platibus.Diagnostics;
using Platibus.Journaling;

namespace Platibus.Config.Extensibility
{
    /// <summary>
    /// Initializes a <see cref="IMessageJournal"/> based on the supplied configuration
    /// </summary>
    public class MessageJournalFactory
    {
        private readonly IDiagnosticService _diagnosticService;

        /// <summary>
        /// Initializes a new <see cref="MessageJournalFactory"/>
        /// </summary>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        public MessageJournalFactory(IDiagnosticService diagnosticService = null)
        {
            _diagnosticService = diagnosticService ?? DiagnosticService.DefaultInstance;
        }

        /// <summary>
        /// Initializes a message journal on the supplied
        /// <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">The journal configuration</param>
        /// <returns>Returns a task whose result is the initialized message journal</returns>
        public async Task<IMessageJournal> InitMessageJournal(JournalingElement configuration)
        {
            var myConfig = configuration ?? new JournalingElement();
            if (string.IsNullOrWhiteSpace(myConfig.Provider))
            {
                await _diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                    {
                        Detail = "Message journal disabled"
                    }.Build());
                return null;
            }

            var provider = GetProvider(myConfig.Provider);
            var messageJournal = await provider.CreateMessageJournal(myConfig);

            var categories = new List<MessageJournalCategory>();
            if (myConfig.JournalSentMessages) categories.Add(MessageJournalCategory.Sent);
            if (myConfig.JournalReceivedMessages) categories.Add(MessageJournalCategory.Received);
            if (myConfig.JournalPublishedMessages) categories.Add(MessageJournalCategory.Published);

            var filteredMessageJournalingService = new FilteredMessageJournal(messageJournal, categories);
            messageJournal = filteredMessageJournalingService;

            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.ComponentInitialization)
                {
                    Detail = "Message journal initialized"
                }.Build());

            return messageJournal;
        }

        private IMessageJournalProvider GetProvider(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName)) throw new ArgumentNullException("providerName");
            return ProviderHelper.GetProvider<IMessageJournalProvider>(providerName);
        }
    }
}