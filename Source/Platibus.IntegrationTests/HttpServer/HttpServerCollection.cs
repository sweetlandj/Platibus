using Xunit;

namespace Platibus.IntegrationTests.HttpServer
{
    [CollectionDefinition(Name)]
    public class HttpServerCollection : ICollectionFixture<HttpServerFixture>
    {
        public const string Name = "IntegrationTests.HttpServer";
    }
}
