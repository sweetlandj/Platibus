using Xunit;

namespace Platibus.UnitTests.LocalDB
{
    [Trait("Category", "UnitTests")]
    [Trait("Dependency", "LocalDB")]
    [Collection(LocalDBCollection.Name)]
    public class LocalDBMessageJournalTests : MessageJournalTests
    {
       
        public LocalDBMessageJournalTests(LocalDBFixture fixture)
            : base(fixture.MessageJournal)
        {
        }
    }
}