using Xunit;
using Xunit.Abstractions;

namespace Platibus.IntegrationTests.HttpServer
{
    [Trait("Category", "LoadTests")]
    [Collection(HttpServerLoadTestCollection.Name)]
    public class HttpServerSendAndReplyLoadTests : SendAndReplyLoadTests
    {
        public HttpServerSendAndReplyLoadTests(HttpServerLoadTestFixture fixture, ITestOutputHelper output)
            : base(fixture.Sender, output)
        {
        }
    }
}