using Xunit;

namespace Platibus.IntegrationTests.OwinMiddleware
{
    [Collection(OwinMiddlewareCollection.Name)]
    public class OwinMiddlewarePubSubTests : PubSubTests
    {
        public OwinMiddlewarePubSubTests(OwinMiddlewareFixture fixture)
            : base(fixture.Sender, fixture.Receiver)
        {
        }
    }
}