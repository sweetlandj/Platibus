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
using Platibus.Config.Extensibility;
using Platibus.Journaling;
using Platibus.Multicast;
#if NET452
using System.Configuration;
#endif
#if NETSTANDARD2_0
using Microsoft.Extensions.Configuration;
#endif

namespace Platibus.SQL
{
    /// <inheritdoc cref="IMessageQueueingServiceProvider" />
    /// <inheritdoc cref="IMessageJournalProvider" />
    /// <inheritdoc cref="ISubscriptionTrackingServiceProvider" />
    /// <summary>
    /// A provider for SQL-based message queueing and subscription tracking services
    /// </summary>
    [Provider("SQL")]
    public class SQLServicesProvider : IMessageQueueingServiceProvider, IMessageJournalProvider, ISubscriptionTrackingServiceProvider
    {
#if NET452
        /// <inheritdoc />
        public async Task<IMessageQueueingService> CreateMessageQueueingService(QueueingElement configuration)
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
                throw new ConfigurationErrorsException($"Connection string settings \"{connectionName}\" not found");
            }

            var securityTokenServiceFactory = new SecurityTokenServiceFactory();
            var securityTokenConfig = configuration.SecurityTokens;
            var securityTokenService = await securityTokenServiceFactory.InitSecurityTokenService(securityTokenConfig);

            var sqlMessageQueueingService = new SQLMessageQueueingService(connectionStringSettings, securityTokenService: securityTokenService);
            sqlMessageQueueingService.Init();
            return sqlMessageQueueingService;
        }

        /// <inheritdoc />
        public Task<IMessageJournal> CreateMessageJournal(JournalingElement configuration)
        {
            var connectionName = configuration.GetString("connectionName");
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ConfigurationErrorsException(
                    "Attribute 'connectionName' is required for SQL message journal");
            }

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionName];
            if (connectionStringSettings == null)
            {
                throw new ConfigurationErrorsException($"Connection string settings \"{connectionName}\" not found");
            }
            var sqlMessageJournalingService = new SQLMessageJournal(connectionStringSettings);
            sqlMessageJournalingService.Init();
            return Task.FromResult<IMessageJournal>(sqlMessageJournalingService);
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
                throw new ConfigurationErrorsException($"Connection string settings \"{connectionName}\" not found");
            }
            var sqlSubscriptionTrackingService = new SQLSubscriptionTrackingService(connectionStringSettings);
            sqlSubscriptionTrackingService.Init();

            var multicast = configuration.Multicast;
            var multicastFactory = new MulticastSubscriptionTrackingServiceFactory();
            return multicastFactory.InitSubscriptionTrackingService(multicast, sqlSubscriptionTrackingService);
        }
#endif
#if NETSTANDARD2_0
        /// <inheritdoc />
        public async Task<IMessageQueueingService> CreateMessageQueueingService(IConfiguration configuration)
        {
            var connectionName = configuration?["connectionName"];
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ConfigurationErrorsException(
                    "Attribute 'connectionName' is required for SQL message queueing service");
            }

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionName];
            if (connectionStringSettings == null)
            {
                throw new ConfigurationErrorsException($"Connection string settings \"{connectionName}\" not found");
            }

            var securityTokenServiceFactory = new SecurityTokenServiceFactory();
            var securityTokensSection = configuration?.GetSection("securityTokens");
            var securityTokenService = await securityTokenServiceFactory.InitSecurityTokenService(securityTokensSection);

            var sqlMessageQueueingService = new SQLMessageQueueingService(connectionStringSettings, securityTokenService: securityTokenService);
            sqlMessageQueueingService.Init();
            return sqlMessageQueueingService;
        }

        /// <inheritdoc />
        public Task<IMessageJournal> CreateMessageJournal(IConfiguration configuration)
        {
            var connectionName = configuration?["connectionName"];
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ConfigurationErrorsException(
                    "Attribute 'connectionName' is required for SQL message journal");
            }

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionName];
            if (connectionStringSettings == null)
            {
                throw new ConfigurationErrorsException($"Connection string settings \"{connectionName}\" not found");
            }
            var sqlMessageJournalingService = new SQLMessageJournal(connectionStringSettings);
            sqlMessageJournalingService.Init();
            return Task.FromResult<IMessageJournal>(sqlMessageJournalingService);
        }

        /// <inheritdoc />
        public Task<ISubscriptionTrackingService> CreateSubscriptionTrackingService(IConfiguration configuration)
        {
            var connectionName = configuration["connectionName"];
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ConfigurationErrorsException(
                    "Attribute 'connectionName' is required for SQL subscription tracking service");
            }

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionName];
            if (connectionStringSettings == null)
            {
                throw new ConfigurationErrorsException($"Connection string settings \"{connectionName}\" not found");
            }
            var sqlSubscriptionTrackingService = new SQLSubscriptionTrackingService(connectionStringSettings);
            sqlSubscriptionTrackingService.Init();

            var multicastSection = configuration.GetSection("multicast");
            var multicastFactory = new MulticastSubscriptionTrackingServiceFactory();
            return multicastFactory.InitSubscriptionTrackingService(multicastSection, sqlSubscriptionTrackingService);
        }
#endif
    }
}