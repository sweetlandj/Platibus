using System.Configuration;
using System.Threading.Tasks;
using Platibus.Config;
using Platibus.Config.Extensibility;

namespace Platibus.SQL
{
    [Provider("SQL")]
    public class SQLServicesProvider : IMessageQueueingServiceProvider, ISubscriptionTrackingServiceProvider
    {
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