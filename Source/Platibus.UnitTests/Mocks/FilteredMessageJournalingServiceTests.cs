using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Platibus.UnitTests.Mocks
{
    public class FilteredMessageJournalingServiceTests : MessageJournalingServiceTests
    {
        protected readonly Mock<IMessageJournalingService> MockMessageJournalingService;

        protected bool JournalSentMessages = true;
        protected bool JournalReceivedMessages = true;
        protected bool JournalPublishedMessages = true;

        public FilteredMessageJournalingServiceTests() 
            : this(new Mock<IMessageJournalingService>())
        {
        }

        public FilteredMessageJournalingServiceTests(Mock<IMessageJournalingService> mockMessageJournalingService)
            : base(mockMessageJournalingService.Object)
        {
            MockMessageJournalingService = mockMessageJournalingService;
        }

        [SetUp]
        public void SetUp()
        {
            MockMessageJournalingService.ResetCalls();
            JournalSentMessages = true;
            JournalReceivedMessages = true;
            JournalPublishedMessages = true;
    }

        [Test]
        public async Task SentMessagesNotJournaledWhenSentMessagesFilteredOut()
        {
            GivenSentMessage();
            GivenSentMessagesOmittedFromJournal();
            await WhenJournalingSentMessage();
            AssertSentMessageIsNotWrittenToJournal();
        }

        [Test]
        public async Task ReceivedMessagesNotJournaledWhenReceivedMessagesFilteredOut()
        {
            GivenReceivedMessage();
            GivenReceivedMessagesOmittedFromJournal();
            await WhenJournalingReceivedMessage();
            AssertReceivedMessageIsNotWrittenToJournal();
        }

        [Test]
        public async Task PublishedMessagesNotJournaledWhenPublishedMessagesFilteredOut()
        {
            GivenPublishedMessage();
            GivenPublishedMessagesOmittedFromJournal();
            await WhenJournalingPublishedMessage();
            AssertPublishedMessageIsNotWrittenToJournal();
        }

        protected void GivenSentMessagesOmittedFromJournal()
        {
            JournalSentMessages = false;
        }

        protected void GivenReceivedMessagesOmittedFromJournal()
        {
            JournalReceivedMessages = false;
        }

        protected void GivenPublishedMessagesOmittedFromJournal()
        {
            JournalPublishedMessages = false;
        }

        protected FilteredMessageJournalingService CreateFilteredMessageJournalingService()
        {
            return new FilteredMessageJournalingService(
                MessageJournalingService,
                JournalSentMessages,
                JournalReceivedMessages, 
                JournalPublishedMessages);
        }
        
        protected override Task WhenJournalingSentMessage()
        {
            return CreateFilteredMessageJournalingService().MessageSent(Message);
        }

        protected override Task WhenJournalingReceivedMessage()
        {
            return CreateFilteredMessageJournalingService().MessageReceived(Message);
        }

        protected override Task WhenJournalingPublishedMessage()
        {
            return CreateFilteredMessageJournalingService().MessagePublished(Message);
        }

        protected override Task AssertSentMessageIsWrittenToJournal()
        {
            MockMessageJournalingService.Verify(x => x.MessageSent(Message, It.IsAny<CancellationToken>()), Times.Once());
            return Task.FromResult(true);
        }

        protected override Task AssertReceivedMessageIsWrittenToJournal()
        {
            MockMessageJournalingService.Verify(x => x.MessageReceived(Message, It.IsAny<CancellationToken>()), Times.Once());
            return Task.FromResult(true);
        }

        protected override Task AssertPublishedMessageIsWrittenToJournal()
        {
            MockMessageJournalingService.Verify(x => x.MessagePublished(Message, It.IsAny<CancellationToken>()), Times.Once());
            return Task.FromResult(true);
        }

        protected void AssertSentMessageIsNotWrittenToJournal()
        {
            MockMessageJournalingService.Verify(x => x.MessageSent(Message, It.IsAny<CancellationToken>()), Times.Never);
        }

        protected void AssertReceivedMessageIsNotWrittenToJournal()
        {
            MockMessageJournalingService.Verify(x => x.MessageReceived(Message, It.IsAny<CancellationToken>()), Times.Never);
        }

        protected void AssertPublishedMessageIsNotWrittenToJournal()
        {
            MockMessageJournalingService.Verify(x => x.MessagePublished(Message, It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
