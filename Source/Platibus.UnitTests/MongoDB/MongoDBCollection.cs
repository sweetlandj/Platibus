using Xunit;

namespace Platibus.UnitTests.MongoDB
{
    [CollectionDefinition(Name)]
    public class MongoDBCollection : ICollectionFixture<MonogDBFixture>
    {
        public const string Name = "UnitTests.MongoDB";
    }
}
