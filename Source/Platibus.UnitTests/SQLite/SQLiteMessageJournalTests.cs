using Xunit;

namespace Platibus.UnitTests.SQLite
{
    [Trait("Category", "UnitTests")]
    [Trait("Dependency", "SQLite")]
    [Collection(SQLiteCollection.Name)]
    public class SQLiteMessageJournalTests : MessageJournalTests
    {
        public SQLiteMessageJournalTests(SQLiteFixture fixture)
            : base(fixture.MessageJournal)
        {
        }
    }
}