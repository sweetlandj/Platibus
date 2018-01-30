using Xunit;

namespace Platibus.UnitTests.RabbitMQ
{
    [CollectionDefinition(Name)]
    public class AesEncryptedRabbitMQCollection : ICollectionFixture<AesEncryptedRabbitMQFixture>
    {
        public const string Name = "UnitTests.AesEncryptedRabbitMQ";
    }
}