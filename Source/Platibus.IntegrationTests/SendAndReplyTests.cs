using System;
using System.Threading.Tasks;
using Xunit;

namespace Platibus.IntegrationTests
{
    public abstract class SendAndReplyTests
    {
        private static readonly Random RNG = new Random();

        protected readonly Task<IBus> Sender;
        protected readonly Task<IBus> Receiver;

        protected TestMessage Message;
        protected SendOptions SendOptions;
        protected ISentMessage SentMessage;

        protected SendAndReplyTests(Task<IBus> sender, Task<IBus> receiver)
        {
            Sender = sender;
            Receiver = receiver;
        }
        
        [Fact]
        public async Task SenderCanReceiveReplies()
        {
            GivenTestMessage();
            await WhenMessageSent();
            await AssertReplyReceived();
        }
        
        protected void GivenTestMessage()
        {
            Message = new TestMessage
            {
                GuidData = Guid.NewGuid(),
                IntData = RNG.Next(0, int.MaxValue),
                StringData = "Hello, world!",
                DateData = DateTime.UtcNow
            };
        }

        protected void GivenNotAuthorizedToSend()
        {
            Message.SimulateAuthorizationFailure = true;
        }

        protected void GivenMessageNotAcknowledged()
        {
            Message.SimulateAcknowledgementFailure = true;
        }

        protected async Task WhenMessageSent()
        {
            var sender = await Sender;
            SentMessage = await sender.Send(Message);
        }

        protected async Task AssertReplyReceived()
        {
            Assert.NotNull(SentMessage);

            var timeout = TimeSpan.FromSeconds(5);
            var reply = await SentMessage.GetReply(timeout);
            Assert.NotNull(reply);
            Assert.IsType<TestReply>(reply);

            var testReply = (TestReply) reply;
            Assert.Equal(Message.GuidData, testReply.GuidData);
            Assert.Equal(Message.IntData, testReply.IntData);
            Assert.Equal(Message.StringData, testReply.StringData);
            Assert.Equal(Message.DateData, testReply.DateData);
        }
    }
}
