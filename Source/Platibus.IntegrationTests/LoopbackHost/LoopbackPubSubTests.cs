using Xunit;

namespace Platibus.IntegrationTests.LoopbackHost
{
    [Collection(LoopbackHostCollection.Name)]
    public class LoopbackPubSubTests : PubSubTests
    {
        public LoopbackPubSubTests(LoopbackHostFixture fixture)
            : base(fixture.Bus, fixture.Bus)
        {
        }
    }
}