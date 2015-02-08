using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Pluribus.UnitTests
{
    class SentMessageExtensionTests
    {
        [Test]
        public async Task Given_SentMessage_When_Reply_Received_Awaited_Task_Should_Return_Reply()
        {
            var memoryCacheReplyHub = new MemoryCacheReplyHub(TimeSpan.FromSeconds(3));
            var messageId = MessageId.Generate();
            var message = new Message(new MessageHeaders { MessageId = messageId }, "Hello, world!");
            var sentMessage = memoryCacheReplyHub.CreateSentMessage(message);
            
            // ReSharper disable once UnusedVariable
            var reply = "Hello yourself!";
            var replyTask = Task.Delay(TimeSpan.FromMilliseconds(500))
                .ContinueWith(t => memoryCacheReplyHub.ReplyReceived(reply, messageId));

            var awaitedReply = await sentMessage
                .GetReply(TimeSpan.FromSeconds(3))
                .ConfigureAwait(false);

            Assert.That(awaitedReply, Is.Not.Null);
            Assert.That(awaitedReply, Is.EqualTo(reply));
        }
    }
}
