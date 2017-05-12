using System;
using System.Threading.Tasks;
using Xunit;

namespace Platibus.IntegrationTests
{
    public abstract class HttpSendAndReplyTests : SendAndReplyTests
    {
        protected HttpSendAndReplyTests(Task<IBus> sender, Task<IBus> receiver) : base(sender, receiver)
        {
        }

        [Fact]
        public async Task UnauthorizedExceptionWhenSendNotAllowed()
        {
            GivenTestMessage();
            GivenNotAuthorizedToSend();
            await Assert.ThrowsAsync<UnauthorizedAccessException>(WhenMessageSent);
        }

        [Fact]
        public async Task MessageNotAcknowledgedExceptionWhenNonCriticalMessageNotAcknowledged()
        {
            GivenTestMessage();
            GivenMessageNotAcknowledged();
            await Assert.ThrowsAsync<MessageNotAcknowledgedException>(WhenMessageSent);
        }
    }
}
