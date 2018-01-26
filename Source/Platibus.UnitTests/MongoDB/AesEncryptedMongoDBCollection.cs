using Xunit;

namespace Platibus.UnitTests.MongoDB
{
    [CollectionDefinition(Name)]
    public class AesEncryptedMongoDBCollection : ICollectionFixture<AesEncryptedMongoDBFixture>
    {
        public const string Name = "UnitTests.AesEncryptedMongoDB";
    }
}