#if NETSTANDARD2_0 || NET461

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

using Microsoft.Extensions.Configuration;
using Platibus.Config.Extensibility;
using Platibus.Diagnostics;
using Platibus.Security;
using Platibus.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Platibus.Config
{
    /// <summary>
    /// Factory class used to initialize <see cref="PlatibusConfiguration"/> objects from
    /// declarative configuration elements in application configuration files.
    /// </summary>
    public class NetStandardConfigurationManager<TConfiguration> where TConfiguration : PlatibusConfiguration
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
    }
}
#endif