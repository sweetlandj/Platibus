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

namespace Pluribus.Config
{
    public static class PluribusConfigurationManager
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Config);

        public static Task<PluribusConfiguration> LoadConfiguration(bool processConfigurationHooks = true)
        {
            return LoadConfiguration("pluribus", processConfigurationHooks);
        }

        public static async Task<PluribusConfiguration> LoadConfiguration(string sectionName, bool processConfigurationHooks = true)
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

            var queueing = configSection.Queueing ?? new QueueingElement();
            switch (queueing.Type)
            {
                case QueueingType.Filesystem:
                    var fsQueueing = queueing.Filesystem ?? new FilesystemQueueingElement();
                    var fsQueueingBaseDir = new DirectoryInfo(GetRootedPath(fsQueueing.Path));
                    var fsQueueingService = new FilesystemMessageQueueingService(fsQueueingBaseDir);
                    fsQueueingService.Init();
                    configuration.MessageQueueingService = fsQueueingService;
                    break;
            }

            var subscriptionTracking = configSection.SubscriptionTracking ?? new SubscriptionTrackingElement();
            switch (subscriptionTracking.Type)
            {
                case SubscriptionTrackingType.Filesystem:
                    var fsSubscriptionTracking = subscriptionTracking.Filesystem ?? new FilesystemSubscriptionsElement();
                    var fsSubscriptionTrackingBaseDir = new DirectoryInfo(GetRootedPath(fsSubscriptionTracking.Path));
                    var fsSubscriptionTrackingService =
                        new FilesystemSubscriptionTrackingService(fsSubscriptionTrackingBaseDir);
                    await fsSubscriptionTrackingService.Init().ConfigureAwait(false);
                    configuration.SubscriptionTrackingService = fsSubscriptionTrackingService;
                    break;
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

        public static string GetRootedPath(string path)
        {
            if (Path.IsPathRooted(path)) return path;

            var appDomainDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appDomainDir, path);
        }

        public static void ProcessConfigurationHooks(PluribusConfiguration configuration)
        {
            if (configuration == null) return;

            var appDomainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var directories = new []
            {
                new DirectoryInfo(appDomainBaseDirectory),
                new DirectoryInfo(Path.Combine(appDomainBaseDirectory, "bin"))
            };

            var filenamePatterns = new [] { "*.dll", "*.exe" };
            var assemblyFiles = directories
                .SelectMany(dir => filenamePatterns, (dir, pattern) => new
                {
                    Directory = dir,
                    FilenamePattern = pattern
                })
                .Where(dir => dir.Directory.Exists)
                .SelectMany(x => x.Directory.GetFiles(x.FilenamePattern, SearchOption.TopDirectoryOnly));

            var hookTypes = new List<Type>();
            foreach (var assemblyFile in assemblyFiles)
            {
                try
                {
                    var assembly = Assembly.ReflectionOnlyLoadFrom(assemblyFile.FullName);
                    Log.DebugFormat("Scanning assembly {0} for configuration hooks...", assembly.GetName().FullName);
                    hookTypes.AddRange(AppDomain.CurrentDomain.Load(assembly.GetName())
                        .GetTypes()
                        .Where(typeof(IConfigurationHook).IsAssignableFrom)
                        .Where(t => !t.IsInterface && !t.IsAbstract));
                }
                catch(Exception ex)
                {
                    Log.WarnFormat("Error scanning assembly file {0}", ex, assemblyFile);
                }
            }

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