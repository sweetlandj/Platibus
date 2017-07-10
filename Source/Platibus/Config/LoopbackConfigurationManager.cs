using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Platibus.Config.Extensibility;
using Platibus.Diagnostics;
using Platibus.Serialization;

namespace Platibus.Config
{
    /// <summary>
    /// Factory class used to initialize <see cref="LoopbackConfiguration"/> objects from
    /// declarative configuration elements in application configuration files.
    /// </summary>
    public class LoopbackConfigurationManager : PlatibusConfigurationManager<LoopbackConfiguration>
    {
        /// <inheritdoc />
        public override async Task Initialize(LoopbackConfiguration configuration, string configSectionName = null)
        {
            var diagnosticService = configuration.DiagnosticService;
            if (string.IsNullOrWhiteSpace(configSectionName))
            {
                configSectionName = "platibus.loopback";
                await diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                    {
                        Detail = "Using default configuration section \"" + configSectionName + "\""
                    }.Build());
            }

            var configSection = ConfigurationManager.GetSection(configSectionName);
            if (configSection == null)
            {
                await diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                    {
                        Detail = "Configuration section \"" + configSectionName +
                                 "\" not found; using default configuration"
                    }.Build());
                configSection = new PlatibusConfigurationSection();
            }

            var platibusConfigSection = configSection as LoopbackConfigurationSection;
            if (platibusConfigSection == null)
            {
                var errorMessage = "Unexpected type for configuration section \"" + configSectionName +
                                   "\": expected " + typeof(LoopbackConfigurationSection) + " but was " +
                                   configSection.GetType();

                await diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationError)
                    {
                        Detail = errorMessage
                    }.Build());

                throw new ConfigurationErrorsException(errorMessage);
            }

            await Initialize(configuration, platibusConfigSection);
        }

        /// <summary>
        /// Initializes the specified <paramref name="configuration"/> object according to the
        /// values in the supplied loopback <paramref name="configSection"/>
        /// </summary>
        /// <param name="configuration">The configuration object to initialize</param>
        /// <param name="configSection">The <see cref="LoopbackConfigurationSection"/>
        /// containing the values used to initialize the Platibus configuration</param>
        public async Task Initialize(LoopbackConfiguration configuration, LoopbackConfigurationSection configSection)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (configSection == null) throw new ArgumentNullException("configSection");

            configuration.ReplyTimeout = configSection.ReplyTimeout;
            configuration.SerializationService = new DefaultSerializationService();
            configuration.MessageNamingService = new DefaultMessageNamingService();
            configuration.DefaultContentType = configSection.DefaultContentType;

            InitializeTopics(configuration, configSection);

            var messageJournalFactory = new MessageJournalFactory(configuration.DiagnosticService);
            configuration.MessageJournal = await messageJournalFactory.InitMessageJournal(configSection.Journaling);

            var mqsFactory = new MessageQueueingServiceFactory(configuration.DiagnosticService);
            configuration.MessageQueueingService = await mqsFactory.InitMessageQueueingService(configSection.Queueing);
        }

        /// <summary>
        /// Initializes topics in the supplied <paramref name="configuration"/> based on the
        /// properties of the specified <paramref name="configSection"/>
        /// </summary>
        /// <param name="configuration">The configuration to initialize</param>
        /// <param name="configSection">The configuration section containing the topic 
        /// properties</param>
        protected virtual void InitializeTopics(LoopbackConfiguration configuration, LoopbackConfigurationSection configSection)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (configSection == null) throw new ArgumentNullException("configSection");
            IEnumerable<TopicElement> topics = configSection.Topics;
            foreach (var topic in topics)
            {
                configuration.AddTopic(topic.Name);
            }
        }
    }
}
