using Xunit;

namespace Platibus.IntegrationTests.RabbitMQHost
{
    [Collection(RabbitMQHostCollection.Name)]
    public class RabbitMQHostSendAndReplyTests : SendAndReplyTests
    {
        public RabbitMQHostSendAndReplyTests(RabbitMQHostFixture fixture)
            : base(fixture.Sender, fixture.Receiver)
        {
        }
    }
}
