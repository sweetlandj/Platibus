using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Platibus.SQL;

namespace Platibus.UnitTests.LocalDB
{
    public class LocalDBMessageJournalingServiceTests : MessageJournalingServiceTests
    {
        private readonly SQLMessageJournalInspector _inspector;

        public LocalDBMessageJournalingServiceTests() 
            : this(LocalDBCollectionFixture.Instance)
        {
        }

        public LocalDBMessageJournalingServiceTests(LocalDBCollectionFixture fixture)
            : this(InitMessageJournalingService(fixture))
        {
        }

        private LocalDBMessageJournalingServiceTests(SQLMessageJournalingService messageJournalingService)
            : base(messageJournalingService)
        {
            _inspector = new SQLMessageJournalInspector(messageJournalingService);
        }

        private static SQLMessageJournalingService InitMessageJournalingService(LocalDBCollectionFixture fixture)
        {
            var messageJournalingService = new SQLMessageJournalingService(fixture.ConnectionProvider, fixture.Dialect);
            messageJournalingService.Init();
            return messageJournalingService;
        }

        protected override async Task AssertSentMessageIsWrittenToJournal()
        {
            var journaledMessages = await _inspector.EnumerateMessages();
            var messageIsJournaled = journaledMessages
                .Any(m => m.Message.Headers.MessageId == Message.Headers.MessageId && m.Category == "Sent");
            Assert.That(messageIsJournaled, Is.True);
        }

        protected override async Task AssertReceivedMessageIsWrittenToJournal()
        {
            var journaledMessages = await _inspector.EnumerateMessages();
            var messageIsJournaled = journaledMessages
                .Any(m => m.Message.Headers.MessageId == Message.Headers.MessageId && m.Category == "Received");
            Assert.That(messageIsJournaled, Is.True);
        }

        protected override async Task AssertPublishedMessageIsWrittenToJournal()
        {
            var journaledMessages = await _inspector.EnumerateMessages();
            var messageIsJournaled = journaledMessages
                .Any(m => m.Message.Headers.MessageId == Message.Headers.MessageId && m.Category == "Published");
            Assert.That(messageIsJournaled, Is.True);
        }
    }
}
