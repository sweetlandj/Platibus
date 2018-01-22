using Xunit;

namespace Platibus.UnitTests.LocalDB
{
    [CollectionDefinition(Name)]
    public class AesEncryptedLocalDBCollection : ICollectionFixture<AesEncryptedLocalDBFixture>
    {
        public const string Name = "UnitTests.AesEncryptedLocalDB";
    }
}