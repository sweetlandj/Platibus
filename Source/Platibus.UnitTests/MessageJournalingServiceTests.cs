using System;
using System.Threading.Tasks;
using Xunit;

namespace Platibus.UnitTests
{
    [Trait("Category", "UnitTests")]
    public abstract class MessageJournalingServiceTests
    {
        protected IMessageJournalingService MessageJournalingService;
        protected Message Message;

        protected MessageJournalingServiceTests(IMessageJournalingService messageJournalingService)
        {
            MessageJournalingService = messageJournalingService;
        }

        [Fact]
        public async Task SentMessagesShouldBeWrittenToJournal()
        {
            GivenSentMessage();
            await WhenJournalingSentMessage();
            await AssertSentMessageIsWrittenToJournal();
        }

        [Fact]
        public async Task ReceivedMessagesShouldBeWrittenToJournal()
        {
            GivenReceivedMessage();
            await WhenJournalingReceivedMessage();
            await AssertReceivedMessageIsWrittenToJournal();
        }

        [Fact]
        public async Task PublishedMessagesShouldBeWrittenToJournal()
        {
            GivenPublishedMessage();
            await WhenJournalingPublishedMessage();
            await AssertPublishedMessageIsWrittenToJournal();
        }

        protected Message GivenSentMessage()
        {
            var messageHeaders = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                Sent = DateTime.UtcNow,
                Origination = new Uri("urn:localhost/platibus0"),
                Destination = new Uri("urn:localhost/platibus1"),
                MessageName = "TestMessage",
                ContentType = "text/plain"
            };
            return Message = new Message(messageHeaders, "Hello, world!");
        }

        protected virtual Task WhenJournalingSentMessage()
        {
            return MessageJournalingService.MessageSent(Message);
        }

        protected abstract Task AssertSentMessageIsWrittenToJournal();

        protected Message GivenReceivedMessage()
        {
            var messageHeaders = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                Sent = DateTime.UtcNow.AddSeconds(-1),
                Received = DateTime.UtcNow,
                Origination = new Uri("urn:localhost/platibus0"),
                Destination = new Uri("urn:localhost/platibus1"),
                MessageName = "TestMessage",
                ContentType = "text/plain"
            };
            return Message = new Message(messageHeaders, "Hello, world!");
        }

        protected virtual Task WhenJournalingReceivedMessage()
        {
            return MessageJournalingService.MessageReceived(Message);
        }

        protected abstract Task AssertReceivedMessageIsWrittenToJournal();

        protected Message GivenPublishedMessage()
        {
            var messageHeaders = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                Published = DateTime.UtcNow,
                Origination = new Uri("urn:localhost/platibus0"),
                MessageName = "TestMessage",
                ContentType = "text/plain",
                Topic = "TestTopic"
            };
            return Message = new Message(messageHeaders, "Hello, world!");
        }

        protected virtual Task WhenJournalingPublishedMessage()
        {
            return MessageJournalingService.MessagePublished(Message);
        }

        protected abstract Task AssertPublishedMessageIsWrittenToJournal();
    }
}
