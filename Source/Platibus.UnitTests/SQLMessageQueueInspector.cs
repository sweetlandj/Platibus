using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Platibus.SQL;

namespace Platibus.UnitTests
{
    class SQLMessageQueueInspector : SQLMessageQueue
    {
        public SQLMessageQueueInspector(SQLMessageQueueingService messageQueueingService, QueueName queueName)
            : base(messageQueueingService.ConnectionProvider, messageQueueingService.Dialect, queueName, new NoopQueueListener())
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

        private class NoopQueueListener : IQueueListener
        {
            public Task MessageReceived(Message message, IQueuedMessageContext context, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(false);
            }
        }
    }
}
