using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using MongoDB.Driver;
using Platibus.MongoDB;
using Xunit;

namespace Platibus.UnitTests.MongoDB
{
    [Trait("Category", "UnitTests")]
    [Trait("Dependency", "MongoDB")]
    [Collection(MongoDBCollection.Name)]
    public class MongoDBMessageQueueingServiceTests : MessageQueueingServiceTests<MongoDBMessageQueueingService>
    {
        private readonly IMongoDatabase _database;

        public MongoDBMessageQueueingServiceTests(MongoDBFixture fixture)
            : base(fixture.MessageQueueingService)
        {
            var client = new MongoClient(fixture.ConnectionStringSettings.ConnectionString);
            _database = client.GetDatabase(MongoDBFixture.DatabaseName);
        }

        private MongoDBMessageQueueInspector Inspect(QueueName queueName)
        {
            var options = new QueueOptions();
            const string collectionName = MongoDBMessageQueueingService.DefaultCollectionName;
            return new MongoDBMessageQueueInspector(_database, queueName, SecurityTokenService, options, collectionName);
        }

        protected override async Task GivenExistingQueuedMessage(QueueName queueName, Message message, IPrincipal principal)
        {
            using (var queueInspector = Inspect(queueName))
            {
                await queueInspector.Init();
                await queueInspector.InsertMessage(message, principal);
            }
        }

        protected override async Task<bool> MessageQueued(QueueName queueName, Message message)
        {
            var messageId = message.Headers.MessageId;
            using (var queueInspector = Inspect(queueName))
            {
                await queueInspector.Init();
                var messagesInQueue = await queueInspector.EnumerateMessages();
                return messagesInQueue.Any(m => m.Message.Headers.MessageId == messageId);
            }
        }

        protected override async Task<bool> MessageDead(QueueName queueName, Message message)
        {
            var messageId = message.Headers.MessageId;
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddSeconds(-5);
            using (var queueInspector = Inspect(queueName))
            {
                await queueInspector.Init();
                var messagesInQueue = await queueInspector.EnumerateAbandonedMessages(startDate, endDate);
                return messagesInQueue.Any(m => m.Message.Headers.MessageId == messageId);
            }
        }
    }
}
