#if NETSTANDARD2_0 || NET461

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
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Platibus.Config.Extensibility;
using Platibus.Diagnostics;
using Platibus.Serialization;

namespace Platibus.Config
{
    /// <inheritdoc />
    /// <summary>
    /// Factory class used to initialize <see cref="T:Platibus.Config.LoopbackConfiguration" /> objects from
    /// declarative configuration elements in application configuration files.
    /// </summary>
    public class NetStandardLoopbackConfigurationManager : NetStandardConfigurationManager<LoopbackConfiguration>
    {
        /// <inheritdoc />
        public override async Task Initialize(LoopbackConfiguration platibusConfiguration, string sectionName = null)
        {
            var diagnosticService = platibusConfiguration.DiagnosticService;
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = "platibus.loopback";
                await diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                    {
                        Detail = "Using default configuration section \"" + sectionName + "\""
                    }.Build());
            }

            var configuration = LoadConfigurationSection(sectionName, diagnosticService);
            await Initialize(platibusConfiguration, configuration);
        }

        public override async Task Initialize(LoopbackConfiguration platibusConfiguration, IConfiguration configuration)
        {
            if (platibusConfiguration == null) throw new ArgumentNullException(nameof(platibusConfiguration));

            await InitializeDiagnostics(platibusConfiguration, configuration);
            var diagnosticService = platibusConfiguration.DiagnosticService;

            platibusConfiguration.ReplyTimeout = configuration?.GetValue<TimeSpan>("replyTimeout") ?? TimeSpan.Zero;
            platibusConfiguration.SerializationService = new DefaultSerializationService();
            platibusConfiguration.MessageNamingService = new DefaultMessageNamingService();
            platibusConfiguration.DefaultContentType = configuration?["defaultContentType"];

            InitializeDefaultSendOptions(platibusConfiguration, configuration);
            InitializeTopics(platibusConfiguration, configuration);

            var messageJournalFactory = new MessageJournalFactory(diagnosticService);
            var journalingSection = configuration?.GetSection("journaling");
            platibusConfiguration.MessageJournal = await messageJournalFactory.InitMessageJournal(journalingSection);

            var mqsFactory = new MessageQueueingServiceFactory(platibusConfiguration.DiagnosticService);
            var queueingSection = configuration?.GetSection("queueing");
            platibusConfiguration.MessageQueueingService = await mqsFactory.InitMessageQueueingService(queueingSection);
        }
    }
}
#endif