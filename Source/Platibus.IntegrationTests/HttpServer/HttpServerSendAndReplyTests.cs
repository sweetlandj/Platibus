using Xunit;

namespace Platibus.IntegrationTests.HttpServer
{
    [Collection(HttpServerCollection.Name)]
    public class HttpServerSendAndReplyTests : HttpSendAndReplyTests
    {
        public HttpServerSendAndReplyTests(HttpServerFixture fixture)
            : base(fixture.Sender, fixture.Receiver)
        {
        }
    }
}
