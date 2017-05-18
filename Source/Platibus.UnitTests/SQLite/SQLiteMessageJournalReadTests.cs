using Xunit;

namespace Platibus.UnitTests.SQLite
{
    [Trait("Category", "UnitTests")]
    [Collection(SQLiteCollection.Name)]
    public class SQLiteMessageJournalReadTests : MessageJournalReadTests
    {       
        public SQLiteMessageJournalReadTests(SQLiteFixture fixture)
            : base(fixture.MessageJournal)
        {
            fixture.DeleteJournaledMessages();
        }
    }
}