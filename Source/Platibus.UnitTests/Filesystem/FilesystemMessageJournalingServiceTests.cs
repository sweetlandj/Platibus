using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Platibus.UnitTests.Filesystem
{
    [Collection(FilesystemCollection.Name)]
    public class FilesystemMessageJournalingServiceTests : MessageJournalingServiceTests
    {
        private readonly FilesystemMessageJournalInspector _inspector;

        public FilesystemMessageJournalingServiceTests(FilesystemFixture fixture)
            : base(fixture.MessageJournalingService)
        {
            _inspector = new FilesystemMessageJournalInspector(fixture.BaseDirectory);
        }
        
        protected override async Task AssertSentMessageIsWrittenToJournal()
        {
            var journaledMessages = await _inspector.EnumerateSentMessages();
            var messageIsJournaled = journaledMessages
                .Any(m => m.Headers.MessageId == Message.Headers.MessageId);
            Assert.True(messageIsJournaled);
        }

        protected override async Task AssertReceivedMessageIsWrittenToJournal()
        {
            var journaledMessages = await _inspector.EnumerateReceivedMessages();
            var messageIsJournaled = journaledMessages
                .Any(m => m.Headers.MessageId == Message.Headers.MessageId);
            Assert.True(messageIsJournaled);
        }

        protected override async Task AssertPublishedMessageIsWrittenToJournal()
        {
            var journaledMessages = await _inspector.EnumeratePublishedMessages();
            var messageIsJournaled = journaledMessages
                .Any(m => m.Headers.MessageId == Message.Headers.MessageId);
            Assert.True(messageIsJournaled);
        }
    }
}
