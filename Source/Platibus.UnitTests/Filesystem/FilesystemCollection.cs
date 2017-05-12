using Xunit;

namespace Platibus.UnitTests.Filesystem
{
    [CollectionDefinition(Name)]
    public class FilesystemCollection : ICollectionFixture<FilesystemFixture>
    {
        public const string Name = "UnitTests.Filesystem";
    }
}
