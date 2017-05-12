using Xunit;

namespace Platibus.IntegrationTests.HttpServer
{
    [CollectionDefinition(Name)]
    public class HttpServerBasicAuthCollection : ICollectionFixture<HttpServerBasicAuthFixture>
    {
        public const string Name = "IntegrationTests.HttpServerBasicAuth";
    }
}
