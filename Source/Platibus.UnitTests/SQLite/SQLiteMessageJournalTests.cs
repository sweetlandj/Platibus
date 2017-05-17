using Xunit;

namespace Platibus.UnitTests.SQLite
{
    [Trait("Category", "UnitTests")]
    [Collection(SQLiteCollection.Name)]
    public class SQLiteMessageJournalTests : MessageJournalTests
    {
        public SQLiteMessageJournalTests(SQLiteFixture fixture)
            : base(fixture.MessageJournal)
        {
        }
    }
}