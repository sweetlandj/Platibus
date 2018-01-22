using Xunit;

namespace Platibus.UnitTests.SQLite
{
    [CollectionDefinition(Name)]
    public class AesEncryptedSQLiteCollection : ICollectionFixture<AesEncryptedSQLiteFixture>
    {
        public const string Name = "UnitTests.AesEncryptedSQLite";
    }
}