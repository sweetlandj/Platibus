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
            
            var replyMessageId = MessageId.Generate();
            var reply = new Message(new MessageHeaders 
            { 
                MessageId = replyMessageId,
                RelatedTo = messageId
            }, "Hello yourself!");

            // ReSharper disable once UnusedVariable
            var replyTask = Task.Delay(TimeSpan.FromMilliseconds(500))
                .ContinueWith(t => memoryCacheReplyHub.ReplyReceived(reply, true));

            var awaitedReply = await sentMessage
                .GetReply(TimeSpan.FromSeconds(3))
                .ConfigureAwait(false);

            Assert.That(awaitedReply, Is.Not.Null);
            Assert.That(awaitedReply, Is.EqualTo(reply));
        }

        [Test]
        public async Task Given_SentMessage_When_Reply_Received_Awaited_Task_Should_Return_Reply_Content()
        {
            var memoryCacheReplyHub = new MemoryCacheReplyHub(TimeSpan.FromSeconds(3));
            var messageId = MessageId.Generate();
            var message = new Message(new MessageHeaders { MessageId = messageId }, "Hello, world!");
            var sentMessage = memoryCacheReplyHub.CreateSentMessage(message);

            var replyMessageId = MessageId.Generate();
            var reply = new Message(new MessageHeaders
            {
                MessageId = replyMessageId,
                RelatedTo = messageId,
                ContentType = "application/json"

            }, "{Message: \"Hello yourself!\"}");

            // ReSharper disable once UnusedVariable
            var replyTask = Task.Delay(TimeSpan.FromMilliseconds(500))
                .ContinueWith(t => memoryCacheReplyHub.ReplyReceived(reply, true));

            var awaitedReply = await sentMessage
                .GetReplyContent<TestReply>(TimeSpan.FromSeconds(3))
                .ConfigureAwait(false);

            Assert.That(awaitedReply, Is.Not.Null);
            Assert.That(awaitedReply, Is.InstanceOf<TestReply>());
            Assert.That(awaitedReply.Message, Is.EqualTo("Hello yourself!"));

        }

        public class TestReply
        {
            public string Message { get; set; }
        }
    }
}
