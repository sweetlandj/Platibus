using Xunit;

namespace Platibus.UnitTests.MongoDB
{
    [CollectionDefinition(Name)]
    public class MongoDBCollection : ICollectionFixture<MongoDBFixture>
    {
        public const string Name = "UnitTests.MongoDB";
    }
}
