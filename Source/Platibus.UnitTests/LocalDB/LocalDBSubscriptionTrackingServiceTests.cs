using Platibus.SQL;
using Xunit;

namespace Platibus.UnitTests.LocalDB
{
    [Trait("Category", "UnitTests")]
    [Trait("Dependency", "LocalDB")]
    [Collection(LocalDBCollection.Name)]
    public class LocalDBSubscriptionTrackingServiceTests : SubscriptionTrackingServiceTests<SQLSubscriptionTrackingService>
    {
        public LocalDBSubscriptionTrackingServiceTests(LocalDBFixture fixture) 
            : base(fixture.SubscriptionTrackingService)
        {
        }
    }
}
