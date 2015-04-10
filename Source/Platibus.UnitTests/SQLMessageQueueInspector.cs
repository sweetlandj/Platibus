using Platibus.SQL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.UnitTests
{
    class SQLMessageQueueInspector : SQLMessageQueue
    {
        public SQLMessageQueueInspector(SQLMessageQueueingService messageQueueingService, QueueName queueName)
            : base(messageQueueingService.ConnectionStringSettings.OpenConnection, messageQueueingService.Dialect, queueName, new NoopQueueListener())
        {
        }

        public SQLQueuedMessage InsertMessage(Message testMessage, IPrincipal senderPrincipal)
        {
            return base.InsertQueuedMessage(testMessage, senderPrincipal);
        }

        public IEnumerable<SQLQueuedMessage> EnumerateMessages()
        {
            return base.SelectQueuedMessages();
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
