using System.Configuration;
using System.Linq;

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

namespace Platibus.Config
{
    /// <summary>
    /// Extension methods for validating <see cref="IPlatibusConfiguration"/>s
    /// </summary>
    public static class PlatibusConfigurationValidationExtensions
    {
        /// <summary>
        /// Checks the specified <paramref name="configuration"/> for omissions, errors, or
        /// inconsistencies.
        /// </summary>
        /// <param name="configuration">The configuration to validate</param>
        public static void Validate(this IPlatibusConfiguration configuration)
        {
            if (configuration == null) return;

            if (configuration.MessageNamingService == null)
            {
                throw new ConfigurationErrorsException("Message naming service is required");
            }

            if (configuration.SerializationService == null)
            {
                throw new ConfigurationErrorsException("Serialization service is required");
            }

            configuration.ValidateSendRules();
            configuration.ValidateSubscriptions();
        }

        /// <summary>
        /// Checks the <see cref="IPlatibusConfiguration.SendRules"/> within the specified 
        /// <paramref name="configuration"/> for errors or inconsistences.
        /// </summary>
        /// <param name="configuration">The configuration to validate</param>
        public static void ValidateSendRules(this IPlatibusConfiguration configuration)
        {
            if (configuration == null) return;

            var endpoints = configuration.Endpoints;
            var sendRuleIndex = 0;
            foreach (var sendRule in configuration.SendRules)
            {
                var targetEndpointNames = sendRule.Endpoints.ToList();
                if (!targetEndpointNames.Any())
                {
                    throw new InvalidSendRuleException("Send rule [" + sendRuleIndex + "] does not reference any endpoints");
                }

                foreach (var targetEndpointName in sendRule.Endpoints)
                {
                    if (!endpoints.Contains(targetEndpointName))
                    {
                        throw new InvalidSendRuleException("Send rule [" + sendRuleIndex + "] references unknown endpoint " + targetEndpointName);
                    }
                }
                sendRuleIndex++;
            }
        }

        /// <summary>
        /// Checks the <see cref="IPlatibusConfiguration.Subscriptions"/> within the specified 
        /// <paramref name="configuration"/> for errors or inconsistences.
        /// </summary>
        /// <param name="configuration">The configuration to validate</param>
        public static void ValidateSubscriptions(this IPlatibusConfiguration configuration)
        {
            if (configuration == null) return;

            var endpoints = configuration.Endpoints;
            var subscriptionIndex = 0;
            foreach (var subscription in configuration.Subscriptions)
            {
                var targetEndpointName = subscription.Endpoint;
                if (!endpoints.Contains(targetEndpointName))
                {
                    throw new InvalidSubscriptionException("Subscription [" + subscriptionIndex + "] references unknown endpoint " + targetEndpointName);
                }
                subscriptionIndex++;
            }
        }
    }
}
