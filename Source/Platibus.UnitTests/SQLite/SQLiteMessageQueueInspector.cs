using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Queueing;
using Platibus.Security;
using Platibus.SQLite;

namespace Platibus.UnitTests.SQLite
{
    internal class SQLiteMessageQueueInspector : SQLiteMessageQueue
    {
        public SQLiteMessageQueueInspector(DirectoryInfo baseDirectory, QueueName queueName, ISecurityTokenService securityTokenService)
            : base(baseDirectory, queueName, new NoopQueueListener(), securityTokenService)
        {
        }

        public Task<QueuedMessage> InsertMessage(Message testMessage, IPrincipal senderPrincipal)
        {
            return InsertQueuedMessage(testMessage, senderPrincipal);
        }

        public Task<IEnumerable<QueuedMessage>> EnumerateMessages()
        {
            return SelectPendingMessages();
        }

        public Task<IEnumerable<QueuedMessage>> EnumerateAbandonedMessages(DateTime startDate, DateTime endDate)
        {
            return SelectDeadMessages(startDate, endDate);
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