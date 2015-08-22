// The MIT License (MIT)
// 
// Copyright (c) 2015 Jesse Sweetland
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Platibus.Config.Extensibility;
using Platibus.Filesystem;
using Platibus.Security;
using Platibus.Serialization;

namespace Platibus.Config
{
    /// <summary>
    /// Factory class used to initialize <see cref="PlatibusConfiguration"/> objects from
    /// declarative configuration elements in application configuration files.
    /// </summary>
    public static class PlatibusConfigurationManager
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Config);

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
        public static Task<PlatibusConfiguration> LoadConfiguration(string sectionName = "platibus",
            bool processConfigurationHooks = true)
        {
            return LoadConfiguration<PlatibusConfiguration>(sectionName, processConfigurationHooks);
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
        public static Task<TConfig> LoadConfiguration<TConfig>(string sectionName, bool processConfigurationHooks = true)
            where TConfig : PlatibusConfiguration, new()
        {
            if (string.IsNullOrWhiteSpace(sectionName)) throw new ArgumentNullException("sectionName");
            var configSection = (PlatibusConfigurationSection) ConfigurationManager.GetSection(sectionName) ??
                                new PlatibusConfigurationSection();
            return LoadConfiguration<TConfig>(configSection, processConfigurationHooks);
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
        public static async Task<TConfig> LoadConfiguration<TConfig>(PlatibusConfigurationSection configSection,
            bool processConfigurationHooks = true) where TConfig : PlatibusConfiguration, new()
        {
            if (configSection == null) throw new ArgumentNullException("configSection");

            var configuration = new TConfig
            {
                SerializationService = new DefaultSerializationService(),
                MessageNamingService = new DefaultMessageNamingService()
            };

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

            IEnumerable<TopicElement> topics = configSection.Topics;
            foreach (var topic in topics)
            {
                configuration.AddTopic(topic.Name);
            }

            // Journaling is optional
            var journaling = configSection.Journaling;
            if (journaling != null && journaling.IsEnabled && !string.IsNullOrWhiteSpace(journaling.Provider))
            {
                configuration.MessageJournalingService = await InitMessageJournalingService(journaling);
            }

            IEnumerable<SendRuleElement> sendRules = configSection.SendRules;
            foreach (var sendRule in sendRules)
            {
                var messageSpec = new MessageNamePatternSpecification(sendRule.NamePattern);
                var endpointName = (EndpointName) sendRule.Endpoint;
                configuration.AddSendRule(new SendRule(messageSpec, endpointName));
            }

            IEnumerable<SubscriptionElement> subscriptions = configSection.Subscriptions;
            foreach (var subscription in subscriptions)
            {
                var endpointName = subscription.Endpoint;
                var topicName = subscription.Topic;
                var ttl = subscription.TTL;
                configuration.AddSubscription(new Subscription(endpointName, topicName, ttl));
            }

            if (processConfigurationHooks)
            {
                ProcessConfigurationHooks(configuration);
            }
            return configuration;
        }

        /// <summary>
        /// Helper method to initialize message queueing services based on the
        /// supplied configuration element
        /// </summary>
        /// <param name="config">The queueing configuration element</param>
        /// <returns>Returns a task whose result is an initialized message queueing service</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is
        /// <c>null</c></exception>
        public static Task<IMessageQueueingService> InitMessageQueueingService(QueueingElement config)
        {
            if (config == null) throw new ArgumentNullException("config");

            var providerName = config.Provider;
            IMessageQueueingServiceProvider provider;
            if (string.IsNullOrWhiteSpace(providerName))
            {
                Log.Debug("No message queueing service provider specified; using default provider...");
                provider = new FilesystemServicesProvider();
            }
            else
            {
                provider = ProviderHelper.GetProvider<IMessageQueueingServiceProvider>(providerName);
            }

            Log.Debug("Initializing message queueing service...");
            return provider.CreateMessageQueueingService(config);
        }

        /// <summary>
        /// Helper method to initialize the message journaling service based on the
        /// supplied configuration element
        /// </summary>
        /// <param name="config">The journaling configuration element</param>
        /// <returns>Returns a task whose result is an initialized message journaling service</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is
        /// <c>null</c></exception>
        public static Task<IMessageJournalingService> InitMessageJournalingService(JournalingElement config)
        {
            if (config == null) throw new ArgumentNullException("config");

            var providerName = config.Provider;
            if (string.IsNullOrWhiteSpace(providerName))
            {
                Log.Debug("No message journaling service provider specified; journaling will be disabled");
                return null;
            }

            var provider = ProviderHelper.GetProvider<IMessageJournalingServiceProvider>(providerName);

            Log.Debug("Initializing message journaling service...");
            return provider.CreateMessageJournalingService(config);
        }

        /// <summary>
        /// Helper method that ensures a path is rooted.
        /// </summary>
        /// <param name="path">The path</param>
        /// <remarks>
        /// If the specified <paramref name="path"/> is rooted then it is returned unchanged.
        /// Otherwise it is appended to the base directory of the application domain to form
        /// an absolute rooted path.
        /// </remarks>
        /// <returns>Returns a rooted path based on the provided <paramref name="path"/> and
        /// the application domain base directory</returns>
        public static string GetRootedPath(string path)
        {
            if (Path.IsPathRooted(path)) return path;

            var appDomainDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appDomainDir, path);
        }

        /// <summary>
        /// Helper method to locate, initialize, and invoke all types inheriting from
        /// <see cref="IConfigurationHook"/> found in the application domain
        /// </summary>
        /// <param name="configuration">The configuration that will be passed to the
        /// configuration hooks</param>
        public static void ProcessConfigurationHooks(PlatibusConfiguration configuration)
        {
            if (configuration == null) return;

            var hookTypes = ReflectionHelper.FindConcreteSubtypes<IConfigurationHook>();
            foreach (var hookType in hookTypes.Distinct())
            {
                try
                {
                    Log.InfoFormat("Processing configuration hook {0}...", hookType.FullName);
                    var hook = (IConfigurationHook) Activator.CreateInstance(hookType);
                    hook.Configure(configuration);
                    Log.InfoFormat("Configuration hook {0} processed successfully.", hookType.FullName);
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unhandled exception in configuration hook {0}", ex, hookType.FullName);
                }
            }
        }
    }
}