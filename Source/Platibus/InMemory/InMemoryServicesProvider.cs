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

using System.Net;
using System.Threading.Tasks;
using Platibus.Config.Extensibility;
using Platibus.Multicast;

#if NET452
using Platibus.Config;
#else
using Microsoft.Extensions.Configuration;
#endif

namespace Platibus.InMemory
{
    /// <summary>
    /// A provider for in-memory message queueing and subscription tracking services
    /// </summary>
    [Provider("InMemory")]
    public class InMemoryServicesProvider : IMessageQueueingServiceProvider, ISubscriptionTrackingServiceProvider
    {
#if NET452
        /// <summary>
        /// Returns an in-memory message queueing service
        /// </summary>
        /// <param name="configuration">The queueing configuration element</param>
        /// <returns>Returns an in-memory message queueing service</returns>
        public Task<IMessageQueueingService> CreateMessageQueueingService(QueueingElement configuration)
        {
            return Task.FromResult<IMessageQueueingService>(new InMemoryMessageQueueingService());
        }

        /// <summary>
        /// Returns an in-memory subscription tracking service
        /// </summary>
        /// <param name="configuration">The subscription tracking configuration element</param>
        /// <returns>Returns an in-memory subscription tracking service</returns>
        public Task<ISubscriptionTrackingService> CreateSubscriptionTrackingService(
            SubscriptionTrackingElement configuration)
        {
            var inMemoryTrackingService = new InMemorySubscriptionTrackingService();
            var multicast = configuration.Multicast;
            if (multicast == null || !multicast.Enabled)
            {
                return Task.FromResult<ISubscriptionTrackingService>(inMemoryTrackingService);
            }

            var multicastTrackingService = new MulticastSubscriptionTrackingService(
                inMemoryTrackingService, multicast.Address, multicast.Port);

            return Task.FromResult<ISubscriptionTrackingService>(multicastTrackingService);
        }
#else
        /// <inheritdoc />
        /// <summary>
        /// Returns an in-memory message queueing service
        /// </summary>
        /// <param name="configuration">The queueing configuration element</param>
        /// <returns>Returns an in-memory message queueing service</returns>
        public Task<IMessageQueueingService> CreateMessageQueueingService(IConfiguration configuration)
        {
            return Task.FromResult<IMessageQueueingService>(new InMemoryMessageQueueingService());
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns an in-memory subscription tracking service
        /// </summary>
        /// <param name="configuration">The subscription tracking configuration element</param>
        /// <returns>Returns an in-memory subscription tracking service</returns>
        public Task<ISubscriptionTrackingService> CreateSubscriptionTrackingService(
            IConfiguration configuration)
        {
            var inMemoryTrackingService = new InMemorySubscriptionTrackingService();
            var multicastSection = configuration?.GetSection("multicast");
            var multicastEnabled = multicastSection?.GetValue("enabled", true) ?? false;
            if (!multicastEnabled)
            {
                return Task.FromResult<ISubscriptionTrackingService>(inMemoryTrackingService);
            }

            var ipAddress = multicastSection.GetValue<IPAddress>("address") ?? IPAddress.Parse("239.255.21.80");
            var port = multicastSection.GetValue<int?>("port") ?? 52181;
            var multicastTrackingService = new MulticastSubscriptionTrackingService(
                inMemoryTrackingService, ipAddress, port);

            return Task.FromResult<ISubscriptionTrackingService>(multicastTrackingService);
        }
#endif


    }
}