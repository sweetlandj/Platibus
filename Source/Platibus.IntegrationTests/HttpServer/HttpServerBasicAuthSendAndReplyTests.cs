using Xunit;

namespace Platibus.IntegrationTests.HttpServer
{
    [Collection(HttpServerBasicAuthCollection.Name)]
    public class HttpServerBasicAuthSendAndReplyTests : HttpSendAndReplyTests
    {
        public HttpServerBasicAuthSendAndReplyTests(HttpServerBasicAuthFixture fixture)
            : base(fixture.Sender, fixture.Receiver)
        {
        }
    }
}