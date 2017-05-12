using Platibus.Filesystem;
using Xunit;

namespace Platibus.UnitTests.Filesystem
{
    [Trait("Category", "UnitTests")]
    [Collection(FilesystemCollection.Name)]
    public class FilesystemSubscriptionTrackingServiceTests : SubscriptionTrackingServiceTests<FilesystemSubscriptionTrackingService>
    {
        public FilesystemSubscriptionTrackingServiceTests(FilesystemFixture fixture)
            : base(fixture.SubscriptionTrackingService)
        {
        }
    }
}
