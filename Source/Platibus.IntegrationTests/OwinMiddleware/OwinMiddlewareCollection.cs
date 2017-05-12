using Xunit;

namespace Platibus.IntegrationTests.OwinMiddleware
{
    [CollectionDefinition(Name)]
    public class OwinMiddlewareCollection : ICollectionFixture<OwinMiddlewareFixture>
    {
        public const string Name = "IntegrationTests.OwinMiddleware";
    }
}
