using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using MongoDB.Driver;
using Platibus.MongoDB;
using Platibus.Queueing;
using Xunit;

namespace Platibus.UnitTests.MongoDB
{
    [Trait("Category", "UnitTests")]
    [Trait("Dependency", "MongoDB")]
    [Collection(AesEncryptedMongoDBCollection.Name)]
    public class AesEncryptedMongoDBMessageQueueingServiceTests : MessageQueueingServiceTests<MongoDBMessageQueueingService>
    {
        private readonly IMongoDatabase _database;

        public AesEncryptedMongoDBMessageQueueingServiceTests(AesEncryptedMongoDBFixture fixture)
            : base(fixture.DiagnosticService, fixture.MessageQueueingService)
        {
            _database = fixture.Database;
        }

        private MongoDBMessageQueueInspector Inspect(QueueName queueName)
        {
            var options = new QueueOptions();
            const string collectionName = MongoDBMessageQueueingService.DefaultCollectionName;
            return new MongoDBMessageQueueInspector(queueName, options, _database, collectionName, SecurityTokenService, null);
        }

        protected override async Task GivenExistingQueuedMessage(QueueName queueName, Message message, IPrincipal principal)
        {
            using (var queueInspector = Inspect(queueName))
            {
                await queueInspector.Init();
                await queueInspector.InsertMessage(new QueuedMessage(message, principal));
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