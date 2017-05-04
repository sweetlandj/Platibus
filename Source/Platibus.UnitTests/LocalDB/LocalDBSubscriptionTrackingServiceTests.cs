using Platibus.SQL;

namespace Platibus.UnitTests.LocalDB
{
    public class LocalDBSubscriptionTrackingServiceTests : SubscriptionTrackingServiceTests<SQLSubscriptionTrackingService>
    {
        public LocalDBSubscriptionTrackingServiceTests()
            : this(LocalDBCollectionFixture.Instance)
        {
            
        }

        private LocalDBSubscriptionTrackingServiceTests(LocalDBCollectionFixture fixture) 
            : base(fixture.SubscriptionTrackingService)
        {
        }
    }
}
