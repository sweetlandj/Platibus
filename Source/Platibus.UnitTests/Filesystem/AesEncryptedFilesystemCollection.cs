using Xunit;

namespace Platibus.UnitTests.Filesystem
{
    [CollectionDefinition(Name)]
    public class AesEncryptedFilesystemCollection : ICollectionFixture<AesEncryptedFilesystemFixture>
    {
        public const string Name = "UnitTests.AesEncryptedFilesystem";
    }
}