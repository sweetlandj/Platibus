
// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
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
using System.Linq;
using System.Threading.Tasks;
using Platibus.Config.Extensibility;
using Platibus.Diagnostics;
using Platibus.Security;
using Platibus.Serialization;
#if NET452
using System.Collections.Generic;
using System.Configuration;
using Platibus.Journaling;
#endif
#if NETSTANDARD2_0
using System.IO;
using Microsoft.Extensions.Configuration;
#endif

namespace Platibus.Config
{
    /// <inheritdoc />
    /// <summary>
    /// Factory class used to initialize <see cref="T:Platibus.Config.PlatibusConfiguration" /> objects from
    /// declarative configuration elements in application configuration files.
    /// </summary>
    public class PlatibusConfigurationManager : PlatibusConfigurationManager<PlatibusConfiguration>
    {
    }

    /// <summary>
    /// Factory class used to initialize <see cref="PlatibusConfiguration"/> objects from
    /// declarative configuration elements in application configuration files.
    /// </summary>
    public class PlatibusConfigurationManager<TConfiguration> where TConfiguration : PlatibusConfiguration
    {
        /// <summary>
        /// Uses reflection to locate, initialize, and invoke all types inheriting from
        /// <see cref="IConfigurationHook"/> or <see cref="IAsyncConfigurationHook"/> found in the 
        /// application domain
        /// </summary>
        /// <param name="configuration">The configuration that will be passed to the configuration
        /// hooks</param>
        public virtual async Task FindAndProcessConfigurationHooks(TConfiguration configuration)
        {
            if (configuration == null) return;

            var diagnosticService = configuration.DiagnosticService;
            var reflectionService = new ReflectionService(diagnosticService);
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
        
        /// <summary>
        /// Helper method to locate, initialize, and invoke all types inheriting from
        /// <see cref="IConfigurationHook"/> found in the application domain
        /// </summary>
        /// <param name="configuration">The configuration that will be passed to the
        ///     configuration hooks</param>
        [Obsolete("Use instance method FindAndProcessConfigurationHooks")]
        public static Task ProcessConfigurationHooks(PlatibusConfiguration configuration)
        {
            if (configuration == null) Task.FromResult(0);
            var configManager = new PlatibusConfigurationManager();
            return configManager.FindAndProcessConfigurationHooks(configuration);
        }

#if NET452
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
        /// <seealso cref="Initialize(TConfiguration,PlatibusConfigurationSection)"/>
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
        
        /// <summary>
        /// Initializes and returns a <see cref="PlatibusConfiguration"/> instance based on
        /// the <see cref="PlatibusConfigurationSection"/> with the specified 
        /// <paramref name="sectionName"/>
        /// </summary>
        /// <param name="sectionName">(Optional) The name of the configuration section 
        /// (default is "platibus")</param>
        /// <param name="processConfigurationHooks">(Optional) Whether to initialize and
        /// process implementations of <see cref="IConfigurationHook"/> found in the
        /// application domain (default is true)</param>
        /// <returns>Returns a task whose result will be an initialized 
        /// <see cref="PlatibusConfiguration"/> object</returns>
        /// <seealso cref="PlatibusConfigurationSection"/>
        /// <seealso cref="IConfigurationHook"/>
        /// <seealso cref="Initialize(TConfiguration,string)"/>
        [Obsolete("Use instance method Initialize")]
        public static async Task<PlatibusConfiguration> LoadConfiguration(string sectionName = null,
            bool processConfigurationHooks = true)
        {
            var configurationManager = new PlatibusConfigurationManager();
            var configuration = new PlatibusConfiguration();
            await configurationManager.Initialize(configuration, sectionName);
            await configurationManager.FindAndProcessConfigurationHooks(configuration);
            if (processConfigurationHooks)
            {
                await configurationManager.FindAndProcessConfigurationHooks(configuration);
            }
            return configuration;
        }
        
        /// <summary>
        /// Initializes and returns a <see cref="PlatibusConfiguration"/> instance based on
        /// the <see cref="LoopbackConfigurationSection"/> with the specified 
        /// <paramref name="sectionName"/>
        /// </summary>
        /// <param name="sectionName">(Optional) The name of the configuration section 
        /// (default is "platibus.loopback")</param>
        /// <param name="processConfigurationHooks">(Optional) Whether to initialize and
        /// process implementations of <see cref="IConfigurationHook"/> found in the
        /// application domain (default is true)</param>
        /// <returns>Returns a task whose result will be an initialized 
        /// <see cref="PlatibusConfiguration"/> object</returns>
        /// <seealso cref="PlatibusConfigurationSection"/>
        /// <seealso cref="IConfigurationHook"/>
        /// <seealso cref="LoopbackConfigurationManager.Initialize(LoopbackConfiguration,string)"/>
        [Obsolete("Use instance method LoopbackConfigurationManager.Initialize")]
        public static async Task<LoopbackConfiguration> LoadLoopbackConfiguration(string sectionName = null,
            bool processConfigurationHooks = true)
        {
            var configurationManager = new LoopbackConfigurationManager();
            var configuration = new LoopbackConfiguration();
            await configurationManager.Initialize(configuration, sectionName);
            if (processConfigurationHooks)
            {
                await configurationManager.FindAndProcessConfigurationHooks(configuration);
            }
            return configuration;
        }

        /// <summary>
        /// Initializes and returns a <typeparamref name="TConfig"/> instance based on
        /// the <see cref="PlatibusConfigurationSection"/> with the specified 
        /// <paramref name="sectionName"/>
        /// </summary>
        /// <typeparam name="TConfig">A type that inherits <see cref="PlatibusConfiguration"/>
        /// and has a default constructor</typeparam>
        /// <param name="sectionName">(Optional) The name of the configuration section 
        /// (default is "platibus")</param>
        /// <param name="processConfigurationHooks">(Optional) Whether to initialize and
        /// process implementations of <see cref="IConfigurationHook"/> found in the
        /// application domain (default is true)</param>
        /// <returns>Returns a task whose result will be an initialized 
        /// <typeparamref name="TConfig"/> object</returns>
        /// <seealso cref="PlatibusConfigurationSection"/>
        /// <seealso cref="IConfigurationHook"/>
        /// <see cref="Initialize(TConfiguration,string)"/>
        [Obsolete("Use instance method Initialize")]
        public static async Task<TConfig> LoadConfiguration<TConfig>(string sectionName, bool processConfigurationHooks = true)
            where TConfig : PlatibusConfiguration, new()
        {
            var configurationManager = new PlatibusConfigurationManager<TConfig>();
            var configuration = new TConfig();
            await configurationManager.Initialize(configuration, sectionName);
            if (processConfigurationHooks)
            {
                await configurationManager.FindAndProcessConfigurationHooks(configuration);
            }
            return configuration;
        }

        /// <summary>
        /// Initializes and returns a <typeparamref name="TConfig"/> instance based on
        /// the supplied <see cref="PlatibusConfigurationSection"/>
        /// </summary>
        /// <typeparam name="TConfig">A type that inherits <see cref="PlatibusConfiguration"/>
        /// and has a default constructor</typeparam>
        /// <param name="configSection">The configuration section</param>
        /// <param name="processConfigurationHooks">(Optional) Whether to initialize and
        /// process implementations of <see cref="IConfigurationHook"/> found in the
        /// application domain (default is true)</param>
        /// <returns>Returns a task whose result will be an initialized 
        /// <typeparamref name="TConfig"/> object</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="configSection"/>
        /// is <c>null</c></exception>
        /// <seealso cref="PlatibusConfigurationSection"/>
        /// <seealso cref="IConfigurationHook"/>
        /// <see cref="Initialize(TConfiguration, PlatibusConfigurationSection)"/>
        [Obsolete("Use instance method Initialize")]
        public static async Task<TConfig> LoadConfiguration<TConfig>(PlatibusConfigurationSection configSection,
            bool processConfigurationHooks = true) where TConfig : PlatibusConfiguration, new()
        {
            var configurationManager = new PlatibusConfigurationManager<TConfig>();
            var configuration = new TConfig();
            await configurationManager.Initialize(configuration, configSection);
            if (processConfigurationHooks)
            {
                await configurationManager.FindAndProcessConfigurationHooks(configuration);
            }
            return configuration;
        }

        /// <summary>
        /// Helper method to initialize message queueing services based on the
        /// supplied configuration element
        /// </summary>
        /// <param name="config">The queueing configuration element</param>
        /// <returns>Returns a task whose result is an initialized message queueing service</returns>
        /// <seealso cref="MessageQueueingServiceFactory.InitMessageQueueingService"/>
        [Obsolete("Use MessageQueueingServiceFactory.InitMessageQueueingService")]
        public static Task<IMessageQueueingService> InitMessageQueueingService(QueueingElement config)
        {
            var factory = new MessageQueueingServiceFactory();
            return factory.InitMessageQueueingService(config);
        }

        /// <summary>
        /// Helper method to initialize security token services based on the
        /// supplied configuration element
        /// </summary>
        /// <param name="config">The security tokens configuration element</param>
        /// <returns>Returns a task whose result is an initialized security token service</returns>
        [Obsolete("Use SecurityTokenServiceFactory.InitSecurityTokenService")]
        public static Task<ISecurityTokenService> InitSecurityTokenService(SecurityTokensElement config)
        {
            var factory = new SecurityTokenServiceFactory();
            return factory.InitSecurityTokenService(config);
        }

        /// <summary>
        /// Helper method to initialize the message journaling service based on the
        /// supplied configuration element
        /// </summary>
        /// <param name="config">The journaling configuration element</param>
        /// <returns>Returns a task whose result is an initialized message journaling service</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is
        /// <c>null</c></exception>
        [Obsolete("Use MessageJournalFactory.InitMessageJournal")]
        public static Task<IMessageJournal> InitMessageJournal(JournalingElement config)
        {
            var factory = new MessageJournalFactory();
            return factory.InitMessageJournal(config);
        }
#else
        /// <summary>
        /// Loads the configuration section of the specified name
        /// </summary>
        /// <param name="configSectionName">The name of the configuration section</param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        /// <returns>Returns the loaded configuration section</returns>
        protected IConfigurationSection LoadConfigurationSection(string configSectionName, IDiagnosticService diagnosticService = null)
        {
            if (string.IsNullOrWhiteSpace(configSectionName)) throw new ArgumentNullException(nameof(configSectionName));
            var myDiagnosticsService = diagnosticService ?? DiagnosticService.DefaultInstance;
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configuration = builder.Build();
            var configSection = configuration.GetSection(configSectionName);
            if (configSection == null)
            {
                myDiagnosticsService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                {
                    Detail = "Configuration section \"" + configSectionName + "\" not found; using default configuration"
                }.Build());
            }
            return configSection;
        }

        /// <summary>
        /// Initializes the specified <paramref name="platibusConfiguration"/> object according to the
        /// values in the <see cref="IConfigurationSection"/> with the specified 
        /// <paramref name="sectionName"/>
        /// </summary>
        /// <param name="platibusConfiguration">The configuration object to initialize</param>
        /// <param name="sectionName">(Optional) The name of the 
        /// <see cref="IConfigurationSection"/> to load</param>
        /// <remarks>
        /// The default configuration section name is "platibus".
        /// </remarks>
        /// <seealso cref="Initialize(TConfiguration,Microsoft.Extensions.Configuration.IConfiguration)"/>
        public virtual Task Initialize(TConfiguration platibusConfiguration, string sectionName = null)
        {
            if (platibusConfiguration == null) throw new ArgumentNullException(nameof(platibusConfiguration));

            var diagnosticService = platibusConfiguration.DiagnosticService;
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = "platibus";
                diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                {
                    Detail = "Using default configuration section \"" + sectionName + "\""
                }.Build());
            }

            var configSection = LoadConfigurationSection(sectionName, diagnosticService);
            return Initialize(platibusConfiguration, configSection);
        }

        /// <summary>
        /// Initializes the specified <paramref name="platibusConfiguration"/> object according to the
        /// values in the supplied <paramref name="configuration"/>
        /// </summary>
        /// <param name="platibusConfiguration">The configuration object to initialize</param>
        /// <param name="configuration">The <see cref="IConfigurationSection"/>
        ///     containing the values used to initialize the Platibus configuration</param>
        public virtual async Task Initialize(TConfiguration platibusConfiguration, IConfiguration configuration)
        {
            if (platibusConfiguration == null) throw new ArgumentNullException(nameof(platibusConfiguration));

            await InitializeDiagnostics(platibusConfiguration, configuration);
            var diagnosticService = platibusConfiguration.DiagnosticService;

            platibusConfiguration.ReplyTimeout = configuration?.GetValue<TimeSpan>("replyTimeout") ?? TimeSpan.Zero;
            platibusConfiguration.SerializationService = new DefaultSerializationService();
            platibusConfiguration.MessageNamingService = new DefaultMessageNamingService();
            platibusConfiguration.DefaultContentType = configuration?["defaultContentType"];

            InitializeDefaultSendOptions(platibusConfiguration, configuration);

            InitializeEndpoints(platibusConfiguration, configuration);
            InitializeTopics(platibusConfiguration, configuration);
            InitializeSendRules(platibusConfiguration, configuration);
            InitializeSubscriptions(platibusConfiguration, configuration);

            var messageJournalFactory = new MessageJournalFactory(diagnosticService);
            var journalingSection = configuration?.GetSection("journaling");
            platibusConfiguration.MessageJournal = await messageJournalFactory.InitMessageJournal(journalingSection);
        }

        protected static void InitializeDefaultSendOptions(TConfiguration platibusConfiguration,
            IConfiguration configuration)
        {
            var sendOptionsSection = configuration?.GetSection("defaultSendOptions");
            if (sendOptionsSection == null) return;

            platibusConfiguration.DefaultSendOptions = new SendOptions
            {
                ContentType = sendOptionsSection["contentType"],
                TTL = sendOptionsSection.GetValue<TimeSpan>("ttl"),
                Synchronous = sendOptionsSection.GetValue<bool>("synchronous")
            };

            var credentialType = sendOptionsSection.GetValue<ClientCredentialType>("credentialType");
            switch (credentialType)
            {
                case ClientCredentialType.Basic:
                    var un = sendOptionsSection["username"];
                    var pw = sendOptionsSection["password"];
                    platibusConfiguration.DefaultSendOptions.Credentials = new BasicAuthCredentials(un, pw);
                    break;
                case ClientCredentialType.Windows:
                case ClientCredentialType.NTLM:
                    platibusConfiguration.DefaultSendOptions.Credentials = new DefaultCredentials();
                    break;
            }
        }

        /// <summary>
        /// Initializes subscriptions in the supplied <paramref name="platibusConfiguration"/> based on the
        /// properties of the specified <paramref name="configuration"/>
        /// </summary>
        /// <param name="platibusConfiguration">The configuration to initialize</param>
        /// <param name="configuration">The configuration section containing the subscription
        /// properties</param>
        protected virtual async Task InitializeDiagnostics(TConfiguration platibusConfiguration,
            IConfiguration configuration)
        {
            var diagnosticsSection = configuration?.GetSection("diagnostics");
            if (diagnosticsSection == null) return;

            var factory = new DiagnosticEventSinkFactory(platibusConfiguration.DiagnosticService);
            var sinksSection = diagnosticsSection.GetSection("sinks");
            var initializations = sinksSection.GetChildren()
                .Select(c => factory.InitDiagnosticEventSink(c));

            var sinks = await Task.WhenAll(initializations);
            foreach (var sink in sinks)
            {
                platibusConfiguration.DiagnosticService.AddSink(sink);
            }
        }

        /// <summary>
        /// Initializes subscriptions in the supplied <paramref name="platibusConfiguration"/> based on the
        /// properties of the specified <paramref name="configuration"/>
        /// </summary>
        /// <param name="platibusConfiguration">The configuration to initialize</param>
        /// <param name="configuration">The configuration section containing the subscription
        /// properties</param>
        protected virtual void InitializeSubscriptions(TConfiguration platibusConfiguration,
            IConfiguration configuration)
        {
            var subscriptionsSection = configuration?.GetSection("subscriptions");
            if (subscriptionsSection == null) return;

            foreach (var subscriptionSection in subscriptionsSection.GetChildren())
            {
                var endpointName = subscriptionSection["endpoint"];
                var topicName = subscriptionSection["topic"];
                var ttl = subscriptionSection.GetValue<TimeSpan>("ttl");
                platibusConfiguration.AddSubscription(new Subscription(endpointName, topicName, ttl));
            }
        }

        /// <summary>
        /// Initializes send rules in the supplied <paramref name="platibusConfiguration"/> based on the
        /// properties of the specified <paramref name="configuration"/>
        /// </summary>
        /// <param name="platibusConfiguration">The configuration to initialize</param>
        /// <param name="configuration">The configuration section containing the send rule 
        /// properties</param>
        protected virtual void InitializeSendRules(TConfiguration platibusConfiguration, IConfiguration configuration)
        {
            var sendRulesSection = configuration?.GetSection("sendRules");
            if (sendRulesSection == null) return;

            foreach (var sendRuleSection in sendRulesSection.GetChildren())
            {
                var messageSpec = new MessageNamePatternSpecification(sendRuleSection["namePattern"]);
                var endpointName = (EndpointName)sendRuleSection["endpoint"];
                platibusConfiguration.AddSendRule(new SendRule(messageSpec, endpointName));
            }
        }

        /// <summary>
        /// Initializes topics in the supplied <paramref name="platibusConfiguration"/> based on the
        /// properties of the specified <paramref name="configuration"/>
        /// </summary>
        /// <param name="platibusConfiguration">The configuration to initialize</param>
        /// <param name="configuration">The configuration section containing the topic 
        /// properties</param>
        protected virtual void InitializeTopics(TConfiguration platibusConfiguration, IConfiguration configuration)
        {
            if (platibusConfiguration == null) throw new ArgumentNullException(nameof(platibusConfiguration));
            var topicsSection = configuration?.GetSection("topics");
            if (topicsSection == null) return;

            foreach (var topic in topicsSection.GetChildren())
            {
                var topicName = topic.Value;
                platibusConfiguration.AddTopic(topicName);
            }
        }

        /// <summary>
        /// Initializes endpoints in the supplied <paramref name="platibusConfiguration"/> based on the
        /// properties of the specified <paramref name="configuration"/>
        /// </summary>
        /// <param name="platibusConfiguration">The configuration to initialize</param>
        /// <param name="configuration">The configuration section containing the endpoint 
        /// properties</param>
        protected virtual void InitializeEndpoints(TConfiguration platibusConfiguration, IConfiguration configuration)
        {
            if (platibusConfiguration == null) throw new ArgumentNullException(nameof(platibusConfiguration));
            var endpointsSection = configuration?.GetSection("endpoints");
            if (endpointsSection == null) return;

            foreach (var endpointSection in endpointsSection.GetChildren())
            {
                var name = endpointSection.Key ??
                           endpointSection["name"];

                var credentialType = endpointSection.GetValue<ClientCredentialType>("credentialType");
                IEndpointCredentials credentials = null;
                switch (credentialType)
                {
                    case ClientCredentialType.Basic:
                        var un = endpointSection["username"];
                        var pw = endpointSection["password"];
                        credentials = new BasicAuthCredentials(un, pw);
                        break;
                    case ClientCredentialType.Windows:
                    case ClientCredentialType.NTLM:
                        credentials = new DefaultCredentials();
                        break;
                }

                var address = endpointSection.GetValue<Uri>("address");
                var endpoint = new Endpoint(address, credentials);
                platibusConfiguration.AddEndpoint(name, endpoint);
            }
        }
#endif

    }
}
