using System;
using System.Collections.Generic;
using System.Text;

namespace Platibus.SQL.Commands
{
    public class MSSQLSelectMessageJournalEntriesCommandBuilder : SelectMessageJournalEntriesCommandBuilder
    {
        public override string CommandText => @"
SELECT TOP (@Count)
    [Id],
    [Category],
    CAST([Timestamp] AS [DateTime]) AS [Timestamp],
    [Headers], 
    [MessageContent]
FROM [PB_MessageJournal] WITH (NOLOCK)
WHERE [Id] >= @Start";
    }
}
