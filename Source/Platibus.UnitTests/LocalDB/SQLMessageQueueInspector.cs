using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Security;
using Platibus.SQL;

namespace Platibus.UnitTests.LocalDB
{
    internal class SQLMessageQueueInspector : SQLMessageQueue
    {
        public SQLMessageQueueInspector(SQLMessageQueueingService messageQueueingService, QueueName queueName, ISecurityTokenService securityTokenService)
            : base(
                messageQueueingService.ConnectionProvider, messageQueueingService.Dialect, queueName,
                new NoopQueueListener(), securityTokenService)
        {
        }

        public Task<SQLQueuedMessage> InsertMessage(Message testMessage, IPrincipal principal)
        {
            return InsertQueuedMessage(testMessage, principal);
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