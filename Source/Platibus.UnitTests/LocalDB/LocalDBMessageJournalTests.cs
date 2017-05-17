using Xunit;

namespace Platibus.UnitTests.LocalDB
{
    [Trait("Category", "UnitTests")]
    [Collection(LocalDBCollection.Name)]
    public class LocalDBMessageJournalTests : MessageJournalTests
    {
       
        public LocalDBMessageJournalTests(LocalDBFixture fixture)
            : base(fixture.MessageJournal)
        {
        }
    }
}