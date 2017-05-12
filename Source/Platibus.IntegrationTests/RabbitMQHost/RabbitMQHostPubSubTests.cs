using Xunit;

namespace Platibus.IntegrationTests.RabbitMQHost
{
    [Collection(RabbitMQHostCollection.Name)]
    public class RabbitMQHostPubSubTests : PubSubTests
    {
        public RabbitMQHostPubSubTests(RabbitMQHostFixture fixture)
            : base(fixture.Sender, fixture.Receiver)
        {
        }
    }
}