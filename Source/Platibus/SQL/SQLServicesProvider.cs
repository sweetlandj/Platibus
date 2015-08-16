using System.Configuration;
using System.Threading.Tasks;
using Platibus.Config;
using Platibus.Config.Extensibility;

namespace Platibus.SQL
{
    /// <summary>
    /// A provider for SQL-based message queueing and subscription tracking services
    /// </summary>
    [Provider("SQL")]
    public class SQLServicesProvider : IMessageQueueingServiceProvider, ISubscriptionTrackingServiceProvider
    {
        /// <summary>
        /// Returns an SQL-based message queueing service
        /// </summary>
        /// <param name="configuration">The queueing configuration element</param>
        /// <returns>Returns an SQL-based message queueing service</returns>
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

        /// <summary>
        /// Returns an SQL-based subscription tracking service
        /// </summary>
        /// <param name="configuration">The subscription tracking configuration element</param>
        /// <returns>Returns an SQL-based subscription tracking service</returns>
        public async Task<ISubscriptionTrackingService> CreateSubscriptionTrackingService(
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
            await sqlSubscriptionTrackingService.Init();
            return sqlSubscriptionTrackingService;
        }
    }
}