using Xunit;

namespace Platibus.UnitTests.LocalDB
{
    [Trait("Category", "UnitTests")]
    [Trait("Dependency", "LocalDB")]
    [Collection(LocalDBCollection.Name)]
    public class LocalDBMessageJournalReadTests : MessageJournalReadTests
    {
       
        public LocalDBMessageJournalReadTests(LocalDBFixture fixture)
            : base(fixture.MessageJournal)
        {
            fixture.DeleteJournaledMessages();
        }
    }
}