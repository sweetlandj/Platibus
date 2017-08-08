using Xunit;

namespace Platibus.UnitTests.MongoDB
{
    [Trait("Category", "UnitTests")]
    [Trait("Dependency", "MongoDB")]
    [Collection(MongoDBCollection.Name)]
    public class MongoDBMessageJournalReadTests : MessageJournalReadTests
    {
        public MongoDBMessageJournalReadTests(MongoDBFixture fixture)
            : base(fixture.MessageJournal)
        {
            fixture.DeleteJournaledMessages();
        }
    }
}