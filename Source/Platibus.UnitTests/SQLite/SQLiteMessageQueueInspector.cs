using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Security;
using Platibus.SQL;
using Platibus.SQLite;

namespace Platibus.UnitTests.SQLite
{
    internal class SQLiteMessageQueueInspector : SQLiteMessageQueue
    {
        public SQLiteMessageQueueInspector(DirectoryInfo baseDirectory, QueueName queueName, IMessageSecurityTokenService messageSecurityTokenService)
            : base(baseDirectory, queueName, new NoopQueueListener(), messageSecurityTokenService)
        {
        }

        public Task<SQLQueuedMessage> InsertMessage(Message testMessage, IPrincipal senderPrincipal)
        {
            return InsertQueuedMessage(testMessage, senderPrincipal);
        }

        public Task<IEnumerable<SQLQueuedMessage>> EnumerateMessages()
        {
            return SelectQueuedMessages();
        }

        public Task<IEnumerable<SQLQueuedMessage>> EnumerateAbandonedMessages(DateTime startDate, DateTime endDate)
        {
            return SelectAbandonedMessages(startDate, endDate);
        }

        private class NoopQueueListener : IQueueListener
        {
            public Task MessageReceived(Message message, IQueuedMessageContext context,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(false);
            }
        }
    }
}