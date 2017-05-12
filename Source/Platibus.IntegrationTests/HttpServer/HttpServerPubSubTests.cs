using Xunit;

namespace Platibus.IntegrationTests.HttpServer
{
    [Collection(HttpServerCollection.Name)]
    public class HttpServerPubSubTests : PubSubTests
    {
        public HttpServerPubSubTests(HttpServerFixture fixture)
            : base(fixture.Sender, fixture.Receiver)
        {
        }
    }
}