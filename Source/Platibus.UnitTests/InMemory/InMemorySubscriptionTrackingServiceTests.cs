using Platibus.InMemory;
using Xunit;

namespace Platibus.UnitTests.InMemory
{
    [Trait("Category", "UnitTests")]
    public class InMemorySubscriptionTrackingServiceTests : SubscriptionTrackingServiceTests<InMemorySubscriptionTrackingService>
    {
        public InMemorySubscriptionTrackingServiceTests() 
            : base(new InMemorySubscriptionTrackingService())
        {
        }
    }
}
