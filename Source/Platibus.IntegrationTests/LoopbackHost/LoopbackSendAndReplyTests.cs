using Xunit;

namespace Platibus.IntegrationTests.LoopbackHost
{
    [Collection(LoopbackHostCollection.Name)]
    public class LoopbackSendAndReplyTests : SendAndReplyTests
    {
        public LoopbackSendAndReplyTests(LoopbackHostFixture fixture)
            : base(fixture.Bus, fixture.Bus)
        {
        }
    }
}
