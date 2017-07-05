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

namespace Platibus.Http
{
    /// <summary>
    /// Helper class for loading HTTP server configuration information
    /// </summary>
    public class HttpServerConfigurationManager : PlatibusConfigurationManager<HttpServerConfiguration>
    {
        /// <summary>
        /// Initializes a <see cref="PlatibusConfigurationManager"/>
        /// </summary>
        /// <param name="diagnosticEventSink">(Optional) A data sink provided by the implementer
        /// to handle diagnostic events related to HTTP server configuration</param>
        public HttpServerConfigurationManager(IDiagnosticEventSink diagnosticEventSink = null) : base(diagnosticEventSink)
        {
        }

        /// <inheritdoc />
        public override async Task Initialize(HttpServerConfiguration configuration, string configSectionName = null)
        {
            if (string.IsNullOrWhiteSpace(configSectionName))
            {
                configSectionName = "platibus.httpserver";
                await DiagnosticEventSink.ReceiveAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                    {
                        Detail = "Using default configuration section \"" + configSectionName + "\""
                    }.Build());
            }

            var configSection = LoadConfigurationSection<HttpServerConfigurationSection>(configSectionName);
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
        public async Task Initialize(HttpServerConfiguration configuration,
            HttpServerConfigurationSection configSection)
        {
            await base.Initialize(configuration, configSection);

            configuration.BaseUri = configSection.BaseUri;
            configuration.ConcurrencyLimit = configSection.ConcurrencyLimit;
            configuration.AuthenticationSchemes = configSection.AuthenticationSchemes.GetFlags();
            configuration.BypassTransportLocalDestination = configSection.BypassTransportLocalDestination;

            var mqsFactory = new MessageQueueingServiceFactory(DiagnosticEventSink);
            var mqsConfig = configSection.Queueing;
            configuration.MessageQueueingService = await mqsFactory.InitMessageQueueingService(mqsConfig);

            var stsFactory = new SubscriptionTrackingServiceFactory(DiagnosticEventSink);
            var stsConfig = configSection.SubscriptionTracking;
            configuration.SubscriptionTrackingService = await stsFactory.InitSubscriptionTrackingService(stsConfig);
        }

        /// <summary>
        /// Initializes an <see cref="HttpServerConfiguration"/> object based on the data in the named
        /// <see cref="HttpServerConfigurationSection"/>
        /// </summary>
        /// <param name="sectionName">(Optional) The name of the configuration section</param>
        /// <returns>Returns a task that will complete when the HTTP server configuration has been 
        /// loaded and initialized and whose result will be the initialized configuration</returns>
        [Obsolete("Use instance method Initialize")]
        public static async Task<HttpServerConfiguration> LoadConfiguration(string sectionName = null)
        {
            var configuration = new HttpServerConfiguration();
            var configManager = new HttpServerConfigurationManager();
            await configManager.Initialize(configuration, sectionName);
            await configManager.FindAndProcessConfigurationHooks(configuration);
            return configuration;
        }

        /// <summary>
        /// Helper method used to initialize an <see cref="ISubscriptionTrackingService"/> based on
        /// the configuration in a <see cref="SubscriptionTrackingElement"/>
        /// </summary>
        /// <param name="config">The configuration element</param>
        /// <returns>Returns a task that will complete when the subscription tracking service has
        /// initialized and whose result is the initialized subscription tracking service</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is <c>null</c>
        /// </exception>
        [Obsolete("Use SubscriptionTrackingServiceFactory.InitSubscriptionTrackingService")]
        public static Task<ISubscriptionTrackingService> InitSubscriptionTrackingService(
            SubscriptionTrackingElement config)
        {
            var factory = new SubscriptionTrackingServiceFactory();
            return factory.InitSubscriptionTrackingService(config);
        }
    }
}