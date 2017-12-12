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

using System.Threading.Tasks;
using Platibus.Config;
#if NET452
using Platibus.Config;
#endif
#if NETSTANDARD2_0
using System.Net;
using Microsoft.Extensions.Configuration;
#endif

namespace Platibus.Multicast
{
    /// <summary>
    /// An object that constructs a <see cref="MulticastSubscriptionTrackingService"/>
    /// based on declarative configuration
    /// </summary>
    public class MulticastSubscriptionTrackingServiceFactory
    {
#if NET452
        /// <summary>
        /// Initializes a new <see cref="MulticastSubscriptionTrackingService"/> based on
        /// the specified <paramref name="configuration"/> and wrapping the supplied
        /// <paramref name="inner"/> subscription tracking service.
        /// </summary>
        /// <param name="configuration">The multicast configuration</param>
        /// <param name="inner">The subscription tracking service that is being wrapped
        /// by the multicast subscription tracking service</param>
        /// <returns></returns>
        public Task<ISubscriptionTrackingService> InitSubscriptionTrackingService(MulticastElement configuration,
            ISubscriptionTrackingService inner)
        {
            if (configuration == null || !configuration.Enabled)
            {
                return Task.FromResult(inner);
            }

            var multicastTrackingService = new MulticastSubscriptionTrackingService(
                inner, configuration.Address, configuration.Port);

            return Task.FromResult<ISubscriptionTrackingService>(multicastTrackingService);
        }
#endif
#if NETSTANDARD2_0
        /// <summary>
        /// Initializes a new <see cref="MulticastSubscriptionTrackingService"/> based on
        /// the specified <paramref name="configuration"/> and wrapping the supplied
        /// <paramref name="inner"/> subscription tracking service.
        /// </summary>
        /// <param name="configuration">The multicast configuration</param>
        /// <param name="inner">The subscription tracking service that is being wrapped
        /// by the multicast subscription tracking service</param>
        /// <returns></returns>
        public Task<ISubscriptionTrackingService> InitSubscriptionTrackingService(IConfiguration configuration,
            ISubscriptionTrackingService inner)
        {
            var multicastSection = configuration?.GetSection("multicast");
            var multicastEnabled = multicastSection?.GetValue<bool>("enabled") ?? false;
            if (!multicastEnabled)
            {
                return Task.FromResult(inner);
            }

            var ipAddress = multicastSection.GetValue("address", IPAddress.Parse(MulticastDefaults.Address));
            var port = multicastSection.GetValue("port", MulticastDefaults.Port);
            var multicastTrackingService = new MulticastSubscriptionTrackingService(inner, ipAddress, port);

            return Task.FromResult<ISubscriptionTrackingService>(multicastTrackingService);
        }
#endif
    }
}
