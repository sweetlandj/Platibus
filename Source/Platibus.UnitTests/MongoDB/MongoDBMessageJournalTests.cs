using Xunit;

namespace Platibus.UnitTests.MongoDB
{
    [Trait("Category", "UnitTests")]
    [Trait("Dependency", "MongoDB")]
    [Collection(MongoDBCollection.Name)]
    public class MongoDBMessageJournalTests : MessageJournalTests
    {
        public MongoDBMessageJournalTests(MongoDBFixture fixture)
            : base(fixture.MessageJournal)
        {
        }
    }
}