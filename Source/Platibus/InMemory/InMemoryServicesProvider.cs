using System.Threading.Tasks;
using Platibus.Config;
using Platibus.Config.Extensibility;

namespace Platibus.InMemory
{
    [Provider("InMemory")]
    public class InMemoryServicesProvider : IMessageQueueingServiceProvider, ISubscriptionTrackingServiceProvider
    {
        public Task<IMessageQueueingService> CreateMessageQueueingService(QueueingElement configuration)
        {
            return Task.FromResult<IMessageQueueingService>(new InMemoryMessageQueueingService());
        }

        public Task<ISubscriptionTrackingService> CreateSubscriptionTrackingService(SubscriptionTrackingElement configuration)
        {
            return Task.FromResult<ISubscriptionTrackingService>(new InMemorySubscriptionTrackingService());
        }
    }
}
