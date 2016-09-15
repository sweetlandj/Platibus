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
using System.Configuration;
using System.Threading.Tasks;
using Common.Logging;
using Platibus.Config;
using Platibus.Config.Extensibility;
using Platibus.Filesystem;

namespace Platibus.Http
{
    /// <summary>
    /// Helper class for loading HTTP server configuration information
    /// </summary>
    public static class HttpServerConfigurationManager
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Config);

        /// <summary>
        /// Initializes an <see cref="HttpServerConfiguration"/> object based on the data in the named
        /// <see cref="HttpServerConfigurationSection"/>
        /// </summary>
        /// <param name="sectionName">(Optional) The name of the configuration section</param>
        /// <returns>Returns a task that will complete when the HTTP server configuration has been 
        /// loaded and initialized and whose result will be the initialized configuration</returns>
        public static async Task<HttpServerConfiguration> LoadConfiguration(string sectionName = null)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = "platibus.httpserver";
            }
            var configSection = (HttpServerConfigurationSection) ConfigurationManager.GetSection(sectionName) ??
                                new HttpServerConfigurationSection();

            var configuration =
                await PlatibusConfigurationManager.LoadConfiguration<HttpServerConfiguration>(sectionName);
            configuration.BaseUri = configSection.BaseUri;
            configuration.ConcurrencyLimit = configSection.ConcurrencyLimit;
            configuration.AuthenticationSchemes = configSection.AuthenticationSchemes.GetFlags();
            configuration.BypassTransportLocalDestination = configSection.BypassTransportLocalDestination;

            var subscriptionTracking = configSection.SubscriptionTracking ?? new SubscriptionTrackingElement();
            configuration.SubscriptionTrackingService = await InitSubscriptionTrackingService(subscriptionTracking);
            configuration.MessageQueueingService = await PlatibusConfigurationManager.InitMessageQueueingService(configSection.Queueing);

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
        public static Task<ISubscriptionTrackingService> InitSubscriptionTrackingService(
            SubscriptionTrackingElement config)
        {
            if (config == null) throw new ArgumentNullException("config");
            var providerName = config.Provider;
            ISubscriptionTrackingServiceProvider provider;
            if (string.IsNullOrWhiteSpace(providerName))
            {
                Log.Debug("No subscription tracking service provider specified; using default provider...");
                provider = new FilesystemServicesProvider();
            }
            else
            {
                provider = ProviderHelper.GetProvider<ISubscriptionTrackingServiceProvider>(providerName);
            }

            Log.Debug("Initializing subscription tracking service...");
            return provider.CreateSubscriptionTrackingService(config);
        }
    }
}