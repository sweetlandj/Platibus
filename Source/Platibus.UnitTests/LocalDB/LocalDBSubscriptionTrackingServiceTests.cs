using Platibus.SQL;
using Xunit;

namespace Platibus.UnitTests.LocalDB
{
    [Collection(LocalDBCollection.Name)]
    public class LocalDBSubscriptionTrackingServiceTests : SubscriptionTrackingServiceTests<SQLSubscriptionTrackingService>
    {
        public LocalDBSubscriptionTrackingServiceTests(LocalDBFixture fixture) 
            : base(fixture.SubscriptionTrackingService)
        {
        }
    }
}
