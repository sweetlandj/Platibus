using Platibus.SQLite;
using Xunit;

namespace Platibus.UnitTests.SQLite
{
    [Collection(SQLiteCollection.Name)]
    public class SQLiteSubscriptionTrackingServiceTests : SubscriptionTrackingServiceTests<SQLiteSubscriptionTrackingService>
    {
        public SQLiteSubscriptionTrackingServiceTests(SQLiteFixture fixture) 
            : base(fixture.SubscriptionTrackingService)
        {
        }
    }
}
