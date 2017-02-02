using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Platibus.SQL;
using Platibus.SQLite;

namespace Platibus.UnitTests.SQLite
{
    internal class SQLiteMessageJournalInspector : SQLiteMessageJournalingService
    {
        public SQLiteMessageJournalInspector(DirectoryInfo baseDirectory)
            : base(baseDirectory)
        {
        }

        public Task<IEnumerable<SQLJournaledMessage>> EnumerateMessages()
        {
            return SelectJournaledMessages();
        }
    }
}