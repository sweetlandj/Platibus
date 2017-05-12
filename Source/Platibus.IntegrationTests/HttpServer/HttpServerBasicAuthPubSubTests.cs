using Xunit;

namespace Platibus.IntegrationTests.HttpServer
{
    [Collection(HttpServerBasicAuthCollection.Name)]
    public class HttpServerBasicAuthPubSubTests : PubSubTests
    {
        public HttpServerBasicAuthPubSubTests(HttpServerBasicAuthFixture fixture)
            : base(fixture.Sender, fixture.Receiver)
        {
        }
    }
}
