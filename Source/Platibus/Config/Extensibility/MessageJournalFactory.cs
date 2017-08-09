// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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

            return new SanitizedMessageJournal(messageJournal);
        }

        private static IMessageJournalProvider GetProvider(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName)) throw new ArgumentNullException("providerName");
            return ProviderHelper.GetProvider<IMessageJournalProvider>(providerName);
        }
    }
}