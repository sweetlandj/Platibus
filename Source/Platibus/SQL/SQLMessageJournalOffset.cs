using Platibus.Journaling;

namespace Platibus.SQL
{
    internal class SQLMessageJournalOffset : MessageJournalOffset
    {
        private readonly long _id;

        public long Id { get { return _id; } }

        public SQLMessageJournalOffset(long id)
        {
            _id = id;
        }

        public override string ToString()
        {
            return _id.ToString("D");
        }
    }
}
