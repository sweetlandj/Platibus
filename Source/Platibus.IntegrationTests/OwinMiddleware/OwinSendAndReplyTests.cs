using Xunit;

namespace Platibus.IntegrationTests.OwinMiddleware
{
    [Collection(OwinMiddlewareCollection.Name)]
    public class OwinSendAndReplyTests : HttpSendAndReplyTests
    {
        public OwinSendAndReplyTests(OwinMiddlewareFixture fixture)
            : base(fixture.Sender, fixture.Receiver)
        {
        }
    }
}
