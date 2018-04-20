#if NET452 || NET461

// The MIT License (MIT)
// 
// Copyright (c) 2018 Jesse Sweetland
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
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Platibus.Config.Extensibility;
using Platibus.Diagnostics;
using Platibus.Security;
using Platibus.Serialization;
using Platibus.Utils;

namespace Platibus.Config
{
    /// <summary>
    /// Factory class used to initialize <see cref="PlatibusConfiguration"/> objects from
    /// declarative configuration elements in application configuration files.
    /// </summary>
    public class NetFrameworkConfigurationManager<TConfiguration> where TConfiguration : PlatibusConfiguration
    {
        /// <summary>
        /// Loads the configuration section of the specified <typeparamref name="TConfigSection">
        /// type and name, appling defaults where appropriate</typeparamref>
        /// </summary>
        /// <typeparam name="TConfigSection">The type of configuration section to load</typeparam>
        /// <param name="configSectionName">The name of the configuration section</param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        /// <returns>Returns the loaded configuration section</returns>
        protected TConfigSection LoadConfigurationSection<TConfigSection>(string configSectionName, IDiagnosticService diagnosticService = null)
            where TConfigSection : ConfigurationSection, new()
        {
            if (string.IsNullOrWhiteSpace(configSectionName)) throw new ArgumentNullException(nameof(configSectionName));
            var myDiagnosticsService = diagnosticService ?? DiagnosticService.DefaultInstance;

            var configSection = ConfigurationManager.GetSection(configSectionName);
            if (configSection == null)
            {
                myDiagnosticsService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                {
                    Detail = "Configuration section \"" + configSectionName + "\" not found; using default configuration"
                }.Build());

                configSection = new TConfigSection();
            }

            if (configSection is TConfigSection typedConfigSection)
            {
                return typedConfigSection;
            }

            var errorMessage = "Unexpected type for configuration section \"" + configSectionName +
                               "\": expected " + typeof(TConfigSection) + " but was " +
                               configSection.GetType();

            myDiagnosticsService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationError)
            {
                Detail = errorMessage
            }.Build());

            throw new ConfigurationErrorsException(errorMessage);
        }

        /// <summary>
        /// Initializes the specified <paramref name="configuration"/> object according to the
        /// values in the <see cref="PlatibusConfigurationSection"/> with the specified 
        /// <paramref name="configSectionName"/>
        /// </summary>
        /// <param name="configuration">The configuration object to initialize</param>
        /// <param name="configSectionName">(Optional) The name of the 
        /// <see cref="PlatibusConfigurationSection"/> to load</param>
        /// <remarks>
        /// The default configuration section name is "platibus".
        /// </remarks>
        /// <seealso cref="Initialize(TConfiguration,Platibus.Config.PlatibusConfigurationSection)"/>
        public virtual Task Initialize(TConfiguration configuration, string configSectionName = null)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var diagnosticService = configuration.DiagnosticService;
            if (string.IsNullOrWhiteSpace(configSectionName))
            {
                configSectionName = "platibus";
                diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                {
                    Detail = "Using default configuration section \"" + configSectionName + "\""
                }.Build());
            }

            var configSection = LoadConfigurationSection<PlatibusConfigurationSection>(configSectionName, diagnosticService);
            return Initialize(configuration, configSection);
        }

        /// <summary>
        /// Initializes the specified <paramref name="configuration"/> object according to the
        /// values in the supplied <paramref name="configSection"/>
        /// </summary>
        /// <param name="configuration">The configuration object to initialize</param>
        /// <param name="configSection">The <see cref="PlatibusConfigurationSection"/>
        /// containing the values used to initialize the Platibus configuration</param>
        public virtual async Task Initialize(TConfiguration configuration, PlatibusConfigurationSection configSection)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (configSection == null) throw new ArgumentNullException(nameof(configSection));

            await InitializeDiagnostics(configuration, configSection);
            var diagnosticService = configuration.DiagnosticService;
            
            configuration.ReplyTimeout = configSection.ReplyTimeout;
            configuration.SerializationService = new DefaultSerializationService();
            configuration.MessageNamingService = new DefaultMessageNamingService();
            configuration.DefaultContentType = configSection.DefaultContentType;

            InitializeDefaultSendOptions(configuration, configSection);

            InitializeEndpoints(configuration, configSection);
            InitializeTopics(configuration, configSection);
            InitializeSendRules(configuration, configSection);
            InitializeSubscriptions(configuration, configSection);

            var messageJournalFactory = new MessageJournalFactory(diagnosticService);
            configuration.MessageJournal = await messageJournalFactory.InitMessageJournal(configSection.Journaling);
        }

        /// <summary>
        /// Uses reflection to locate, initialize, and invoke all types inheriting from
        /// <see cref="IConfigurationHook"/> or <see cref="IAsyncConfigurationHook"/> found in the 
        /// application domain
        /// </summary>
        /// <param name="configuration">The configuration that will be passed to the configuration
        /// hooks</param>
        [Obsolete]
        public virtual async Task FindAndProcessConfigurationHooks(TConfiguration configuration)
        {
            if (configuration == null) return;

            var diagnosticService = configuration.DiagnosticService;
            var reflectionService = new DefaultReflectionService(diagnosticService);
            var hookTypes = reflectionService.FindConcreteSubtypes<IConfigurationHook>();
            foreach (var hookType in hookTypes.Distinct())
            {
                try
                {
                    await diagnosticService.EmitAsync(
                        new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationHook)
                        {
                            Detail = "Found configuration hook " + hookType,
                        }.Build());

                    var hook = (IConfigurationHook)Activator.CreateInstance(hookType);
                    hook.Configure(configuration);

                    await diagnosticService.EmitAsync(
                        new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationHook)
                        {
                            Detail = "Configuration hook " + hookType + " processed successfully"
                        }.Build());
                }
                catch (Exception ex)
                {
                    diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationError)
                    {
                        Detail = "Unhandled exception processing configuration hook " + hookType,
                        Exception = ex
                    }.Build());
                }
            }

            var asyncHookTypes = reflectionService.FindConcreteSubtypes<IAsyncConfigurationHook>();
            foreach (var hookType in asyncHookTypes.Distinct())
            {
                try
                {
                    await diagnosticService.EmitAsync(new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationHook)
                    {
                        Detail = "Found async configuration hook " + hookType,
                    }.Build());

                    var hook = (IAsyncConfigurationHook)Activator.CreateInstance(hookType);
                    await hook.Configure(configuration);

                    await diagnosticService.EmitAsync(new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationHook)
                    {
                        Detail = "Async configuration hook " + hookType + " processed successfully"
                    }.Build());
                }
                catch (Exception ex)
                {
                    diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationError)
                    {
                        Detail = "Unhandled exception processing async configuration hook " + hookType,
                        Exception = ex
                    }.Build());
                }
            }
        }

        private static void InitializeDefaultSendOptions(TConfiguration configuration,
            PlatibusConfigurationSection configSection)
        {
            if (configSection.DefaultSendOptions == null) return;

            configuration.DefaultSendOptions = new SendOptions
            {
                ContentType = configSection.DefaultSendOptions.ContentType,
                TTL = configSection.DefaultSendOptions.TTL,
                Synchronous = configSection.DefaultSendOptions.Synchronous
            };

            switch (configSection.DefaultSendOptions.CredentialType)
            {
                case ClientCredentialType.Basic:
                    var un = configSection.DefaultSendOptions.Username;
                    var pw = configSection.DefaultSendOptions.Password;
                    configuration.DefaultSendOptions.Credentials = new BasicAuthCredentials(un, pw);
                    break;
                case ClientCredentialType.Windows:
                case ClientCredentialType.NTLM:
                    configuration.DefaultSendOptions.Credentials = new DefaultCredentials();
                    break;
            }
        }
        
        /// <summary>
        /// Initializes subscriptions in the supplied <paramref name="configuration"/> based on the
        /// properties of the specified <paramref name="configSection"/>
        /// </summary>
        /// <param name="configuration">The configuration to initialize</param>
        /// <param name="configSection">The configuration section containing the subscription
        /// properties</param>
        protected virtual async Task InitializeDiagnostics(TConfiguration configuration,
            PlatibusConfigurationSection configSection)
        {
            var diagnosticsConfig = configSection.Diagnostics;
            if (diagnosticsConfig == null) return;

            var factory = new DiagnosticEventSinkFactory(configuration.DiagnosticService);
            IEnumerable<DiagnosticEventSinkElement> sinkConfigs = diagnosticsConfig.Sinks;
            foreach (var sinkConfig in sinkConfigs)
            {
                var sink = await factory.InitDiagnosticEventSink(sinkConfig);
                configuration.DiagnosticService.AddSink(sink);
            }
        }

        /// <summary>
        /// Initializes subscriptions in the supplied <paramref name="configuration"/> based on the
        /// properties of the specified <paramref name="configSection"/>
        /// </summary>
        /// <param name="configuration">The configuration to initialize</param>
        /// <param name="configSection">The configuration section containing the subscription
        /// properties</param>
        protected virtual void InitializeSubscriptions(TConfiguration configuration,
            PlatibusConfigurationSection configSection)
        {
            IEnumerable<SubscriptionElement> subscriptions = configSection.Subscriptions;
            foreach (var subscription in subscriptions)
            {
                var endpointName = subscription.Endpoint;
                var topicName = subscription.Topic;
                var ttl = subscription.TTL;
                configuration.AddSubscription(new Subscription(endpointName, topicName, ttl));
            }
        }
        
        /// <summary>
        /// Initializes send rules in the supplied <paramref name="configuration"/> based on the
        /// properties of the specified <paramref name="configSection"/>
        /// </summary>
        /// <param name="configuration">The configuration to initialize</param>
        /// <param name="configSection">The configuration section containing the send rule 
        /// properties</param>
        protected virtual void InitializeSendRules(TConfiguration configuration, PlatibusConfigurationSection configSection)
        {
            IEnumerable<SendRuleElement> sendRules = configSection.SendRules;
            foreach (var sendRule in sendRules)
            {
                var messageSpec = new MessageNamePatternSpecification(sendRule.NamePattern);
                var endpointName = (EndpointName) sendRule.Endpoint;
                configuration.AddSendRule(new SendRule(messageSpec, endpointName));
            }
        }

        /// <summary>
        /// Initializes topics in the supplied <paramref name="configuration"/> based on the
        /// properties of the specified <paramref name="configSection"/>
        /// </summary>
        /// <param name="configuration">The configuration to initialize</param>
        /// <param name="configSection">The configuration section containing the topic 
        /// properties</param>
        protected virtual void InitializeTopics(TConfiguration configuration, PlatibusConfigurationSection configSection)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (configSection == null) throw new ArgumentNullException(nameof(configSection));
            IEnumerable<TopicElement> topics = configSection.Topics;
            foreach (var topic in topics)
            {
                configuration.AddTopic(topic.Name);
            }
        }

        /// <summary>
        /// Initializes endpoints in the supplied <paramref name="configuration"/> based on the
        /// properties of the specified <paramref name="configSection"/>
        /// </summary>
        /// <param name="configuration">The configuration to initialize</param>
        /// <param name="configSection">The configuration section containing the endpoint 
        /// properties</param>
        protected virtual void InitializeEndpoints(TConfiguration configuration, PlatibusConfigurationSection configSection)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (configSection == null) throw new ArgumentNullException(nameof(configSection));
            
            IEnumerable<EndpointElement> endpoints = configSection.Endpoints;
            foreach (var endpointConfig in endpoints)
            {
                IEndpointCredentials credentials = null;
                switch (endpointConfig.CredentialType)
                {
                    case ClientCredentialType.Basic:
                        var un = endpointConfig.Username;
                        var pw = endpointConfig.Password;
                        credentials = new BasicAuthCredentials(un, pw);
                        break;
                    case ClientCredentialType.Windows:
                    case ClientCredentialType.NTLM:
                        credentials = new DefaultCredentials();
                        break;
                }

                var endpoint = new Endpoint(endpointConfig.Address, credentials);
                configuration.AddEndpoint(endpointConfig.Name, endpoint);
            }
        }
    }
}
#endif