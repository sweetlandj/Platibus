using Platibus.Filesystem;

namespace Platibus.UnitTests.Filesystem
{
    public class FilesystemSubscriptionTrackingServiceTests : SubscriptionTrackingServiceTests<FilesystemSubscriptionTrackingService>
    {
        public FilesystemSubscriptionTrackingServiceTests()
            : this(FilesystemCollectionFixture.Instance)
        {
        }

        public FilesystemSubscriptionTrackingServiceTests(FilesystemCollectionFixture fixture)
            : base(fixture.SubscriptionTrackingService)
        {
        }
    }
}
