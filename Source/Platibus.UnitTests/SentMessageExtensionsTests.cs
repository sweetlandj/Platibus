using System;
using System.Threading.Tasks;
using Xunit;

namespace Platibus.UnitTests
{
    internal class SentMessageExtensionsTests
    {
       [Fact]
        public async Task FirstReplyIsReturned()
        {
            var memoryCacheReplyHub = new MemoryCacheReplyHub(TimeSpan.FromSeconds(3));
            var messageId = MessageId.Generate();
            var message = new Message(new MessageHeaders {MessageId = messageId}, "Hello, world!");
            var sentMessage = memoryCacheReplyHub.CreateSentMessage(message);

            const string reply = "Hello yourself!";
            var replyTask = Task.Delay(TimeSpan.FromMilliseconds(500))
                .ContinueWith(t => memoryCacheReplyHub.ReplyReceived(reply, messageId));

            var awaitedReply = await sentMessage.GetReply(TimeSpan.FromSeconds(3));
            await replyTask;

            Assert.NotNull(awaitedReply);
            Assert.Equal(reply, awaitedReply);
        }
    }
}