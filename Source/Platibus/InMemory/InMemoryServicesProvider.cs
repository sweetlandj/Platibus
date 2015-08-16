using System.Threading.Tasks;
using Platibus.Config;
using Platibus.Config.Extensibility;

namespace Platibus.InMemory
{
    /// <summary>
    /// A provider for in-memory message queueing and subscription tracking services
    /// </summary>
    [Provider("InMemory")]
    public class InMemoryServicesProvider : IMessageQueueingServiceProvider, ISubscriptionTrackingServiceProvider
    {
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
            return Task.FromResult<ISubscriptionTrackingService>(new InMemorySubscriptionTrackingService());
        }
    }
}