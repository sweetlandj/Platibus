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

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Platibus.Config;
using Platibus.Config.Extensibility;
using Platibus.Diagnostics;

namespace Platibus.Http
{
    /// <inheritdoc />
    /// <summary>
    /// Helper class for loading HTTP server configuration information
    /// </summary>
    public class NetStandardHttpServerConfigurationManager : NetStandardConfigurationManager<HttpServerConfiguration>
    {
        /// <inheritdoc />
        public override async Task Initialize(HttpServerConfiguration platibusConfiguration, string sectionName = null)
        {
            var diagnosticService = platibusConfiguration.DiagnosticService;
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = "platibus.httpserver";
                await diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                    {
                        Detail = "Using default configuration section \"" + sectionName + "\""
                    }.Build());
            }

            var configuration = LoadConfigurationSection(sectionName, diagnosticService);
            await Initialize(platibusConfiguration, configuration);
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes the supplied HTTP server <paramref name="platibusConfiguration" /> based on the
        /// properties of the provided <paramref name="configuration" />
        /// </summary>
        /// <param name="platibusConfiguration">The configuration to initialize</param>
        /// <param name="configuration">The configuration section whose properties are to be used
        /// to initialize the <paramref name="platibusConfiguration" /></param>
        /// <returns>Returns a task that completes when the configuration has been initialized</returns>
        public override async Task Initialize(HttpServerConfiguration platibusConfiguration, IConfiguration configuration)
        {
            await base.Initialize(platibusConfiguration, configuration);

            platibusConfiguration.BaseUri = configuration?.GetValue<Uri>("baseUri");
            platibusConfiguration.ConcurrencyLimit = configuration?.GetValue("concurrencyLimit", 0) ?? 0;

            InitializeAuthenticationSchemes(platibusConfiguration, configuration);
            platibusConfiguration.BypassTransportLocalDestination = configuration?.GetValue<bool>("bypassTransportLocalDestination") ?? false;

            var mqsFactory = new MessageQueueingServiceFactory(platibusConfiguration.DiagnosticService);
            var queueingSection = configuration?.GetSection("queueing");
            platibusConfiguration.MessageQueueingService = await mqsFactory.InitMessageQueueingService(queueingSection);

            var stsFactory = new SubscriptionTrackingServiceFactory(platibusConfiguration.DiagnosticService);
            var stSection = configuration?.GetSection("subscriptionTracking");
            platibusConfiguration.SubscriptionTrackingService = await stsFactory.InitSubscriptionTrackingService(stSection);
        }

        private static void InitializeAuthenticationSchemes(HttpServerConfiguration platibusConfiguration, IConfiguration configuration)
        {
            var authenticationSchemes = AuthenticationSchemes.Anonymous;
            var authenticationSchemesSection = configuration?.GetSection("authenticationSchemes");
            if (authenticationSchemesSection == null) return;

            foreach (var authenticationSchemeSection in authenticationSchemesSection.GetChildren())
            {
                if (Enum.TryParse(authenticationSchemeSection.Value, out AuthenticationSchemes authenticationScheme))
                {
                    authenticationSchemes |= authenticationScheme;
                }
            }
            platibusConfiguration.AuthenticationSchemes = authenticationSchemes;
        }
    }
}
#endif