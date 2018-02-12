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
using Microsoft.Extensions.Configuration;
using Platibus.Config;
using Platibus.Config.Extensibility;
using Platibus.Diagnostics;

namespace Platibus.AspNetCore
{
    /// <inheritdoc />
    /// <summary>
    /// Factory class used to initialize <see cref="T:Platibus.AspNetCore.AspNetCoreConfiguration" /> objects from
    /// declarative configuration elements in web configuration files.
    /// </summary>
    public class AspNetCoreConfigurationManager : NetStandardConfigurationManager<AspNetCoreConfiguration>
    {
        public override async Task Initialize(AspNetCoreConfiguration platibusConfiguration, string configSectionName = null)
        {
            var diagnosticService = platibusConfiguration.DiagnosticService;
            if (string.IsNullOrWhiteSpace(configSectionName))
            {
                configSectionName = "platibus";
                await diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                    {
                        Detail = $"Using default configuration section \"{configSectionName}\""
                    }.Build());
            }

            var configSection = LoadConfigurationSection(configSectionName, diagnosticService);
            await Initialize(platibusConfiguration, configSection);

            platibusConfiguration.BaseUri = configSection?.GetValue<Uri>("baseUri");
            platibusConfiguration.BypassTransportLocalDestination = configSection?.GetValue("BypassTransportLocalDestination", true) ?? true;

            var mqsFactory = new MessageQueueingServiceFactory(platibusConfiguration.DiagnosticService);
            var mqsConfig = configSection?.GetSection("queueing");
            platibusConfiguration.MessageQueueingService = await mqsFactory.InitMessageQueueingService(mqsConfig);

            var stsFactory = new SubscriptionTrackingServiceFactory(platibusConfiguration.DiagnosticService);
            var stsConfig = configSection?.GetSection("subscriptionTracking");
            platibusConfiguration.SubscriptionTrackingService = await stsFactory.InitSubscriptionTrackingService(stsConfig);
        }
    }
}