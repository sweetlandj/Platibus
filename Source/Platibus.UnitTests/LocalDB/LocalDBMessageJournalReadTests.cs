using Xunit;

namespace Platibus.UnitTests.LocalDB
{
    [Trait("Category", "UnitTests")]
    [Collection(LocalDBCollection.Name)]
    public class LocalDBMessageJournalReadTests : MessageJournalReadTests
    {
       
        public LocalDBMessageJournalReadTests(LocalDBFixture fixture)
            : base(fixture.MessageJournal)
        {
        }
    }
}