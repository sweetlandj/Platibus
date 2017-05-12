using Xunit;

namespace Platibus.UnitTests.SQLite
{
    [CollectionDefinition(Name)]
    public class SQLiteCollection : ICollectionFixture<SQLiteFixture>
    {
        public const string Name = "UnitTests.SQLite";
    }
}
