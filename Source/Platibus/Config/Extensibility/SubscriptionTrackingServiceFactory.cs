// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
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

using System.Threading.Tasks;
#if NETSTANDARD2_0 || NET461
using Microsoft.Extensions.Configuration;
#endif
using Platibus.Diagnostics;
using Platibus.Filesystem;

namespace Platibus.Config.Extensibility
{
    /// <summary>
    /// Initializes a <see cref="ISubscriptionTrackingService"/> based on the supplied configuration
    /// </summary>
    public class SubscriptionTrackingServiceFactory
    {
        private readonly IDiagnosticService _diagnosticService;
        private readonly IProviderService _providerService;

        /// <summary>
        /// Initializes a new <see cref="SubscriptionTrackingServiceFactory"/>
        /// </summary>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        public SubscriptionTrackingServiceFactory(IDiagnosticService diagnosticService = null)
        {
            _diagnosticService = diagnosticService ?? DiagnosticService.DefaultInstance;
            _providerService = new ReflectionBasedProviderService(_diagnosticService);
        }

#if NET452 || NET461
        /// <summary>
        /// Initializes a subscription tracking service based on the supplied
        /// <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">The subscription tracking configuration</param>
        /// <returns>Returns a task whose result is the initialized subscription tracking service</returns>
        public async Task<ISubscriptionTrackingService> InitSubscriptionTrackingService(SubscriptionTrackingElement configuration)
        {
            var myConfig = configuration ?? new SubscriptionTrackingElement();
            var provider = await GetProvider(myConfig.Provider);

            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.ComponentInitialization)
                {
                    Detail = "Initializing subscription tracking service"
                }.Build());

            var subscriptionTrackingService = await provider.CreateSubscriptionTrackingService(myConfig);

            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.ComponentInitialization)
                {
                    Detail = $"Message queueing service {subscriptionTrackingService.GetType().FullName} initialized"
                }.Build());

            return subscriptionTrackingService;
        }
#endif
#if NETSTANDARD2_0 || NET461
        /// <summary>
        /// Initializes a subscription tracking service based on the supplied
        /// <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">The subscription tracking configuration</param>
        /// <returns>Returns a task whose result is the initialized subscription tracking service</returns>
        public async Task<ISubscriptionTrackingService> InitSubscriptionTrackingService(IConfigurationSection configuration)
        {
            var providerName = configuration?["provider"];
            var provider = await GetProvider(providerName);

            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.ComponentInitialization)
                {
                    Detail = "Initializing subscription tracking service"
                }.Build());

            var subscriptionTrackingService = await provider.CreateSubscriptionTrackingService(configuration);

            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.ComponentInitialization)
                {
                    Detail = $"Message queueing service {subscriptionTrackingService.GetType().FullName} initialized"
                }.Build());

            return subscriptionTrackingService;
        }
#endif

        private async Task<ISubscriptionTrackingServiceProvider> GetProvider(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                await _diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                    {
                        Detail = "Using default filesystem-based subscription tracking service provider"
                    }.Build());

                return new FilesystemServicesProvider();
            }
            return _providerService.GetProvider<ISubscriptionTrackingServiceProvider>(providerName);
        }
    }
}