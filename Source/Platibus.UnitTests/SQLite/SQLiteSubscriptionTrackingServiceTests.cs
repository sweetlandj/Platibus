using Platibus.SQLite;

namespace Platibus.UnitTests.SQLite
{
    public class SQLiteSubscriptionTrackingServiceTests : SubscriptionTrackingServiceTests<SQLiteSubscriptionTrackingService>
    {
        public SQLiteSubscriptionTrackingServiceTests()
            : this(SQLiteCollectionFixture.Instance)
        {
        }

        private SQLiteSubscriptionTrackingServiceTests(SQLiteCollectionFixture fixture) 
            : base(fixture.SubscriptionTrackingService)
        {
        }
    }
}
