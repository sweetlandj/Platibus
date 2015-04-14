using Platibus.Config;
using Platibus.Config.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.InMemory
{
    [Provider("InMemory")]
    public class InMemoryServicesProvider : IMessageQueueingServiceProvider, ISubscriptionTrackingServiceProvider
    {
        public IMessageQueueingService CreateMessageQueueingService(QueueingElement configuration)
        {
            return new InMemoryMessageQueueingService();
        }

        public ISubscriptionTrackingService CreateSubscriptionTrackingService(SubscriptionTrackingElement configuration)
        {
            return new InMemorySubscriptionTrackingService();
        }
    }
}
