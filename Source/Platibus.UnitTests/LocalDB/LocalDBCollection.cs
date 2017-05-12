using Xunit;

namespace Platibus.UnitTests.LocalDB
{
    [CollectionDefinition(Name)]
    public class LocalDBCollection : ICollectionFixture<LocalDBFixture>
    {
        public const string Name = "UnitTests.LocalDB";
    }
}
