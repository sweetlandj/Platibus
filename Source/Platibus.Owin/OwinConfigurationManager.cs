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
using System.Threading.Tasks;
using Platibus.Config;
using Platibus.Config.Extensibility;
using Platibus.Diagnostics;

namespace Platibus.Owin
{
    /// <summary>
    /// Factory class used to initialize <see cref="OwinConfiguration"/> objects from
    /// declarative configuration elements in web configuration files.
    /// </summary>
    public class OwinConfigurationManager : PlatibusConfigurationManager<OwinConfiguration>
    {
        public override async Task Initialize(OwinConfiguration configuration, string configSectionName = null)
        {
            var diagnosticService = configuration.DiagnosticService;
            if (string.IsNullOrWhiteSpace(configSectionName))
            {
                configSectionName = "platibus.owin";
                await diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                    {
                        Detail = "Using default configuration section \"" + configSectionName + "\""
                    }.Build());
            }

            var configSection = LoadConfigurationSection<OwinConfigurationSection>(configSectionName, diagnosticService);
            await Initialize(configuration, configSection);
        }
        
        /// <summary>
        /// Initializes the supplied HTTP server <paramref name="configuration"/> based on the
        /// properties of the provided <paramref name="configSection"/>
        /// </summary>
        /// <param name="configuration">The configuration to initialize</param>
        /// <param name="configSection">The configuration section whose properties are to be used
        /// to initialize the <paramref name="configuration"/></param>
        /// <returns>Returns a task that completes when the configuration has been initialized</returns>
        public async Task Initialize(OwinConfiguration configuration, OwinConfigurationSection configSection)
        {
            await base.Initialize(configuration, configSection);
            configuration.BaseUri = configSection.BaseUri;
            configuration.BypassTransportLocalDestination = configSection.BypassTransportLocalDestination;

            var mqsFactory = new MessageQueueingServiceFactory(configuration.DiagnosticService);
            var mqsConfig = configSection.Queueing;
            configuration.MessageQueueingService = await mqsFactory.InitMessageQueueingService(mqsConfig);

            var stsFactory = new SubscriptionTrackingServiceFactory(configuration.DiagnosticService);
            var stsConfig = configSection.SubscriptionTracking;
            configuration.SubscriptionTrackingService = await stsFactory.InitSubscriptionTrackingService(stsConfig);
        }

        /// <summary>
        /// Initializes and returns a <see cref="OwinConfiguration"/> instance based on
        /// the <see cref="OwinConfigurationSection"/> with the specified 
        /// <paramref name="sectionName"/>
        /// </summary>
        /// <param name="sectionName">(Optional) The name of the configuration section 
        /// (default is "platibus.owin")</param>
        /// <param name="processConfigurationHooks">(Optional) Whether to initialize and
        /// process implementations of <see cref="IConfigurationHook"/> found in the
        /// application domain (default is true)</param>
        /// <returns>Returns a task whose result will be an initialized 
        /// <see cref="PlatibusConfiguration"/> object</returns>
        /// <seealso cref="PlatibusConfigurationSection"/>
        /// <seealso cref="IConfigurationHook"/>
        public new static async Task<OwinConfiguration> LoadConfiguration(string sectionName = null,
            bool processConfigurationHooks = true)
        {
            var configManager = new OwinConfigurationManager();
            var configuration = new OwinConfiguration();
            await configManager.Initialize(configuration, sectionName);
            if (processConfigurationHooks)
            {
                await configManager.FindAndProcessConfigurationHooks(configuration);
            }
            return configuration;
        }

        /// <summary>
        /// Helper method to initialize the subscription tracking service based on the
        /// supplied configuration element
        /// </summary>
        /// <param name="config">The subscription tracking configuration element</param>
        /// <returns>Returns a task whose result is an initialized subscription tracking service</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is
        /// <c>null</c></exception>
        [Obsolete("Use SubscriptionTrackingServiceFactory.InitSubscriptionTrackingService")]
        public static Task<ISubscriptionTrackingService> InitSubscriptionTrackingService(
            SubscriptionTrackingElement config)
        {
            var factory = new SubscriptionTrackingServiceFactory();
            return factory.InitSubscriptionTrackingService(config);
        }
    }
}