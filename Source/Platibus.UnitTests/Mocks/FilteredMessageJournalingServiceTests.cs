using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

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

        private FilteredMessageJournalingServiceTests(Mock<IMessageJournalingService> mockMessageJournalingService)
            : base(mockMessageJournalingService.Object)
        {
            MockMessageJournalingService = mockMessageJournalingService;
            MockMessageJournalingService
                .Setup(x => x.MessageSent(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));

            MockMessageJournalingService
                .Setup(x => x.MessageReceived(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));

            MockMessageJournalingService
                .Setup(x => x.MessagePublished(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
        }

        [Fact]
        public async Task SentMessagesNotJournaledWhenSentMessagesFilteredOut()
        {
            GivenSentMessage();
            GivenSentMessagesOmittedFromJournal();
            await WhenJournalingSentMessage();
            AssertSentMessageIsNotWrittenToJournal();
        }

        [Fact]
        public async Task ReceivedMessagesNotJournaledWhenReceivedMessagesFilteredOut()
        {
            GivenReceivedMessage();
            GivenReceivedMessagesOmittedFromJournal();
            await WhenJournalingReceivedMessage();
            AssertReceivedMessageIsNotWrittenToJournal();
        }

        [Fact]
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
