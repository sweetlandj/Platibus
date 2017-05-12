using Xunit;

namespace Platibus.IntegrationTests.RabbitMQHost
{
    [CollectionDefinition(Name)]
    public class RabbitMQHostCollection : ICollectionFixture<RabbitMQHostFixture>
    {
        public const string Name = "IntegrationTests.RabbitMQHost";
    }
}
