using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Platibus.UnitTests.SQLite
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class SQLiteMessageJournalingServiceTests : MessageJournalingServiceTests
    {
        private readonly SQLiteMessageJournalInspector _inspector;

        public SQLiteMessageJournalingServiceTests() 
            : this(SQLiteCollectionFixture.Instance)
        {
        }

        public SQLiteMessageJournalingServiceTests(SQLiteCollectionFixture fixture)
            : base(fixture.MessageJournalingService)
        {
            _inspector = new SQLiteMessageJournalInspector(fixture.BaseDirectory);
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
