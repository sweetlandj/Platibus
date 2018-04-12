namespace Platibus.UnitTests.Journaling
{
    public class MessageJournalStubTests : MessageJournalTests
    {
        public MessageJournalStubTests() : base(new MessageJournalStub())
        {
        }
    }
}
