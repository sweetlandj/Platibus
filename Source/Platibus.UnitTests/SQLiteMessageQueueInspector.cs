using Platibus.SQL;
using Platibus.SQLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            return base.InsertQueuedMessage(testMessage, senderPrincipal);
        }

        public Task<IEnumerable<SQLQueuedMessage>> EnumerateMessages()
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
