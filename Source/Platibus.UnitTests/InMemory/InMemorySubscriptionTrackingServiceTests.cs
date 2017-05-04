using Platibus.InMemory;

namespace Platibus.UnitTests.InMemory
{
    public class InMemorySubscriptionTrackingServiceTests : SubscriptionTrackingServiceTests<InMemorySubscriptionTrackingService>
    {
        public InMemorySubscriptionTrackingServiceTests() 
            : base(new InMemorySubscriptionTrackingService())
        {
        }
    }
}
