// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
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
using Pluribus.Filesystem;
using Pluribus.Serialization;
using System.Reflection;
using Pluribus.Config.Extensibility;

namespace Pluribus.Config
{
    public static class PluribusConfigurationManager
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Config);

        public static PluribusConfiguration LoadConfiguration(bool processConfigurationHooks = true)
        {
            return LoadConfiguration("pluribus", processConfigurationHooks);
        }

        public static PluribusConfiguration LoadConfiguration(string sectionName, bool processConfigurationHooks = true)
        {
            var configuration = new PluribusConfiguration();

            var configSection = (PluribusConfigurationSection) ConfigurationManager.GetSection(sectionName) ??
                                new PluribusConfigurationSection();
            configuration.BaseUri = configSection.BaseUri;
            configuration.SerializationService = new DefaultSerializationService();
            configuration.MessageNamingService = new DefaultMessageNamingService();

            IEnumerable<EndpointElement> endpoints = configSection.Endpoints;
            foreach (var dest in endpoints)
            {
                var endpoint = new Endpoint(dest.Address);
                configuration.AddEndpoint(dest.Name, endpoint);
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
                configuration.MessageJournalingService = InitMessageJournalingService(journaling);
            }

            var queueing = configSection.Queueing ?? new QueueingElement();
            configuration.MessageQueueingService = InitMessageQueueingService(queueing);

            var subscriptionTracking = configSection.SubscriptionTracking ?? new SubscriptionTrackingElement();
            configuration.SubscriptionTrackingService = InitSubscriptionTrackingService(subscriptionTracking);

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

        public static IMessageQueueingService InitMessageQueueingService(QueueingElement config)
        {
            var providerName = config.Provider;
            IMessageQueueingServiceProvider provider;
            if (string.IsNullOrWhiteSpace(providerName))
            {
                Log.Debug("No message queueing service provider specified; using default provider...");
                provider = new FilesystemServicesProvider();
            }
            else
            {
                provider = GetProvider<IMessageQueueingServiceProvider>(providerName);
            }

            Log.Debug("Initializing message queueing service...");
            return provider.CreateMessageQueueingService(config);
        }

        public static IMessageJournalingService InitMessageJournalingService(JournalingElement config)
        {
            var providerName = config.Provider;
            if (string.IsNullOrWhiteSpace(providerName))
            {
                Log.Debug("No message journaling service provider specified; journaling will be disabled");
                return null;
            }
            
            var provider = GetProvider<IMessageJournalingServiceProvider>(providerName);
            
            Log.Debug("Initializing message journaling service...");
            return provider.CreateMessageJournalingService(config);
        }

        public static ISubscriptionTrackingService InitSubscriptionTrackingService(SubscriptionTrackingElement config)
        {
            var providerName = config.Provider;
            ISubscriptionTrackingServiceProvider provider;
            if (string.IsNullOrWhiteSpace(providerName))
            {
                Log.Debug("No subscription tracking service provider specified; using default provider...");
                provider = new FilesystemServicesProvider();
            }
            else
            {
                provider = GetProvider<ISubscriptionTrackingServiceProvider>(providerName);
            }

            Log.Debug("Initializing subscription tracking service...");
            return provider.CreateSubscriptionTrackingService(config);
        }

        public static TProvider GetProvider<TProvider>(string providerName)
        {
            var providerType = Type.GetType(providerName);
            if (providerType == null)
            {
                Log.DebugFormat("Looking for provider \"{0}\"...", providerName);
                var providers = ReflectionHelper
                    .FindConcreteSubtypes<TProvider>()
                    .With<ProviderAttribute>(a => string.Equals(providerName, a.Name, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!providers.Any()) throw new ProviderNotFoundException(providerName);
                if (providers.Count > 1) throw new MultipleProvidersFoundException(providerName, providers);

                providerType = providers.First();
            }

            Log.DebugFormat("Found provider type \"{0}\"", providerType.FullName);
            return (TProvider)Activator.CreateInstance(providerType);
        }

        public static string GetRootedPath(string path)
        {
            if (Path.IsPathRooted(path)) return path;

            var appDomainDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appDomainDir, path);
        }

        public static void ProcessConfigurationHooks(PluribusConfiguration configuration)
        {
            if (configuration == null) return;

            var hookTypes = ReflectionHelper.FindConcreteSubtypes<IConfigurationHook>();
            foreach (var hookType in hookTypes.Distinct())
            {
                try
                {
                    Log.InfoFormat("Processing configuration hook {0}...", hookType.FullName);
                    var hook = (IConfigurationHook)Activator.CreateInstance(hookType);
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