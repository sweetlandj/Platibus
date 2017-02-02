using System.Collections.Generic;
using System.Threading.Tasks;
using Platibus.SQL;

namespace Platibus.UnitTests.LocalDB
{
    internal class SQLMessageJournalInspector : SQLMessageJournalingService
    {
        public SQLMessageJournalInspector(SQLMessageJournalingService messageJournalingService)
            : base(messageJournalingService.ConnectionProvider, messageJournalingService.Dialect)
        {
        }

        public Task<IEnumerable<SQLJournaledMessage>> EnumerateMessages()
        {
            return SelectJournaledMessages();
        }
    }
}