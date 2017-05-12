using Xunit;

namespace Platibus.IntegrationTests.LoopbackHost
{
    [CollectionDefinition(Name)]
    public class LoopbackHostCollection : ICollectionFixture<LoopbackHostFixture>
    {
        public const string Name = "IntegrationTests.LoopbackHost";
    }
}
