using Platibus.SQLite;
using Xunit;

namespace Platibus.UnitTests.SQLite
{
    [Trait("Category", "UnitTests")]
    [Collection(SQLiteCollection.Name)]
    public class SQLiteSubscriptionTrackingServiceTests : SubscriptionTrackingServiceTests<SQLiteSubscriptionTrackingService>
    {
        public SQLiteSubscriptionTrackingServiceTests(SQLiteFixture fixture) 
            : base(fixture.SubscriptionTrackingService)
        {
        }
    }
}
