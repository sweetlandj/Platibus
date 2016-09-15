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

namespace Platibus.Owin
{
    /// <summary>
    /// Factory class used to initialize <see cref="OwinConfiguration"/> objects from
    /// declarative configuration elements in web configuration files.
    /// </summary>
    public static class OwinConfigurationManager
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Config);

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
        public static async Task<OwinConfiguration> LoadConfiguration(string sectionName = null,
            bool processConfigurationHooks = true)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = "platibus.owin";
            }

            var configSection = (OwinConfigurationSection) ConfigurationManager.GetSection(sectionName) ??
                                new OwinConfigurationSection();

            var configuration = await PlatibusConfigurationManager.LoadConfiguration<OwinConfiguration>(sectionName, processConfigurationHooks);
            configuration.BaseUri = configSection.BaseUri;
            configuration.BypassTransportLocalDestination = configSection.BypassTransportLocalDestination;

            var subscriptionTracking = configSection.SubscriptionTracking ?? new SubscriptionTrackingElement();
            configuration.SubscriptionTrackingService = await InitSubscriptionTrackingService(subscriptionTracking);
            configuration.MessageQueueingService = await PlatibusConfigurationManager.InitMessageQueueingService(configSection.Queueing);

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
        public static Task<ISubscriptionTrackingService> InitSubscriptionTrackingService(
            SubscriptionTrackingElement config)
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
                provider = ProviderHelper.GetProvider<ISubscriptionTrackingServiceProvider>(providerName);
            }

            Log.Debug("Initializing subscription tracking service...");
            return provider.CreateSubscriptionTrackingService(config);
        }
    }
}