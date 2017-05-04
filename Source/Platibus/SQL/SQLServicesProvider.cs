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

using System.Configuration;
using System.Threading.Tasks;
using Platibus.Config;
using Platibus.Config.Extensibility;
using Platibus.Multicast;

namespace Platibus.SQL
{
    /// <summary>
    /// A provider for SQL-based message queueing and subscription tracking services
    /// </summary>
    [Provider("SQL")]
    public class SQLServicesProvider : IMessageQueueingServiceProvider, IMessageJournalingServiceProvider, ISubscriptionTrackingServiceProvider
    {
        /// <inheritdoc />
        public Task<IMessageQueueingService> CreateMessageQueueingService(QueueingElement configuration)
        {
            var connectionName = configuration.GetString("connectionName");
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ConfigurationErrorsException(
                    "Attribute 'connectionName' is required for SQL message queueing service");
            }

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionName];
            if (connectionStringSettings == null)
            {
                throw new ConfigurationErrorsException("Connection string settings \"" + connectionName + "\" not found");
            }
            var sqlMessageQueueingService = new SQLMessageQueueingService(connectionStringSettings);
            sqlMessageQueueingService.Init();
            return Task.FromResult<IMessageQueueingService>(sqlMessageQueueingService);
        }

        /// <inheritdoc />
        public Task<IMessageJournalingService> CreateMessageJournalingService(JournalingElement configuration)
        {
            var connectionName = configuration.GetString("connectionName");
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ConfigurationErrorsException(
                    "Attribute 'connectionName' is required for SQL message journaling service");
            }

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionName];
            if (connectionStringSettings == null)
            {
                throw new ConfigurationErrorsException("Connection string settings \"" + connectionName + "\" not found");
            }
            var sqlMessageJournalingService = new SQLMessageJournalingService(connectionStringSettings);
            sqlMessageJournalingService.Init();
            return Task.FromResult<IMessageJournalingService>(sqlMessageJournalingService);
        }
        
        /// <inheritdoc />
        public Task<ISubscriptionTrackingService> CreateSubscriptionTrackingService(
            SubscriptionTrackingElement configuration)
        {
            var connectionName = configuration.GetString("connectionName");
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ConfigurationErrorsException(
                    "Attribute 'connectionName' is required for SQL subscription tracking service");
            }

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionName];
            if (connectionStringSettings == null)
            {
                throw new ConfigurationErrorsException("Connection string settings \"" + connectionName + "\" not found");
            }
            var sqlSubscriptionTrackingService = new SQLSubscriptionTrackingService(connectionStringSettings);
            sqlSubscriptionTrackingService.Init();

            var multicast = configuration.Multicast;
            if (multicast == null || !multicast.Enabled)
            {
                return Task.FromResult<ISubscriptionTrackingService>(sqlSubscriptionTrackingService);
            }

            var multicastTrackingService = new MulticastSubscriptionTrackingService(
                sqlSubscriptionTrackingService, multicast.Address, multicast.Port);

            return Task.FromResult<ISubscriptionTrackingService>(multicastTrackingService);
        }
    }
}