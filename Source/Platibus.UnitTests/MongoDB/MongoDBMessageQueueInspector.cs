using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Platibus.MongoDB;
using Platibus.Queueing;
using Platibus.Security;

namespace Platibus.UnitTests.MongoDB
{
    internal class MongoDBMessageQueueInspector : MongoDBMessageQueue
    {
        public MongoDBMessageQueueInspector(IMongoDatabase database, QueueName queueName, ISecurityTokenService securityTokenService, QueueOptions options = null, string collectionName = null) 
            : base(database, queueName, new NoopQueueListener(), securityTokenService, options, collectionName)
        {
        }

        public override Task Init(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(0);
        }

        public Task<QueuedMessage> InsertMessage(Message testMessage, IPrincipal principal)
        {
            return InsertQueuedMessage(testMessage, principal);
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
