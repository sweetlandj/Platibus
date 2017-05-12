using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Platibus.UnitTests.SQLite
{
    [Collection(SQLiteCollection.Name)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class SQLiteMessageJournalingServiceTests : MessageJournalingServiceTests
    {
        private readonly SQLiteMessageJournalInspector _inspector;
        
        public SQLiteMessageJournalingServiceTests(SQLiteFixture fixture)
            : base(fixture.MessageJournalingService)
        {
            _inspector = new SQLiteMessageJournalInspector(fixture.BaseDirectory);
        }
        
        protected override async Task AssertSentMessageIsWrittenToJournal()
        {
            var journaledMessages = await _inspector.EnumerateMessages();
            var messageIsJournaled = journaledMessages
                .Any(m => m.Message.Headers.MessageId == Message.Headers.MessageId && m.Category == "Sent");
            Assert.True(messageIsJournaled);
        }

        protected override async Task AssertReceivedMessageIsWrittenToJournal()
        {
            var journaledMessages = await _inspector.EnumerateMessages();
            var messageIsJournaled = journaledMessages
                .Any(m => m.Message.Headers.MessageId == Message.Headers.MessageId && m.Category == "Received");
            Assert.True(messageIsJournaled);
        }

        protected override async Task AssertPublishedMessageIsWrittenToJournal()
        {
            var journaledMessages = await _inspector.EnumerateMessages();
            var messageIsJournaled = journaledMessages
                .Any(m => m.Message.Headers.MessageId == Message.Headers.MessageId && m.Category == "Published");
            Assert.True(messageIsJournaled);
        }
    }
}
