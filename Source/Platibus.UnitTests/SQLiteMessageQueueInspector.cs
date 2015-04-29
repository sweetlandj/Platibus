using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Platibus.SQL;
using Platibus.SQLite;

namespace Platibus.UnitTests
{
    class SQLiteMessageQueueInspector : SQLiteMessageQueue
    {
        public SQLiteMessageQueueInspector(DirectoryInfo baseDirectory, QueueName queueName)
            : base(baseDirectory, queueName, new NoopQueueListener())
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
