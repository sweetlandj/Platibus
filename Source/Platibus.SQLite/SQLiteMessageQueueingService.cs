using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.SQLite
{
    public class SQLiteMessageQueueingService : IMessageQueueingService
    {
        public SQLiteMessageQueueingService(ConnectionStringSettings connectionStringSettings)
        {

        }

        public void Init()
        {

        }

        public Task CreateQueue(QueueName queueName, IQueueListener listener, QueueOptions options = default(QueueOptions))
        {
            throw new NotImplementedException();
        }

        public Task EnqueueMessage(QueueName queueName, Message message, System.Security.Principal.IPrincipal senderPrincipal)
        {
            throw new NotImplementedException();
        }
    }
}
