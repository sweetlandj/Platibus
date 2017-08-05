using Xunit;

namespace Platibus.IntegrationTests.HttpServer
{
    [CollectionDefinition(Name)]
    public class HttpServerLoadTestCollection : ICollectionFixture<HttpServerLoadTestFixture>
    {
        public const string Name = "IntegrationTests.HttpServerLoadTest";
    }
}