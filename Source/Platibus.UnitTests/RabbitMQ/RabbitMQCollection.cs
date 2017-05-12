using Xunit;

namespace Platibus.UnitTests.RabbitMQ
{
    [CollectionDefinition(Name)]
    public class RabbitMQCollection : ICollectionFixture<RabbitMQFixture>
    {
        public const string Name = "UnitTests.RabbitMQ";
    }
}
