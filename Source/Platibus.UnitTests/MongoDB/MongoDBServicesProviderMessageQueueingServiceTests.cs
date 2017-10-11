using Moq;
using Platibus.Config;
using Platibus.MongoDB;
using System;
using System.Configuration;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace Platibus.UnitTests.MongoDB
{
    [Trait("Category", "UnitTests")]
    [Trait("Dependency", "MongoDB")]
    [Collection(MongoDBCollection.Name)]
    public class MongoDBServicesProviderMessageQueueingServiceTests
    {
        private readonly ConnectionStringSettings _connectionStringSettings;

        public Message Message;
        public QueueName Queue = Guid.NewGuid().ToString();
        public IQueueListener QueueListener = new Mock<IQueueListener>().Object;

        public QueueingElement Queueing = new QueueingElement();

        public MongoDBServicesProviderMessageQueueingServiceTests(MongoDBFixture fixture)
        {
            _connectionStringSettings = fixture.ConnectionStringSettings;
            Queueing.SetAttribute("connectionName", _connectionStringSettings.Name);

            Message = new Message(new MessageHeaders
            {
                MessageId = MessageId.Generate()
            }, "MongoDBServicesProviderMessageQueueingServiceTests");
        }

        [Fact]
        public async Task DatabaseNameCanBeOverridden()
        {
            const string databaseOverride = "pbtest1";
            GivenDatabaseOverride(databaseOverride);
            await WhenMessageEnqueued();
            await AssertMessageDocumentInserted(MongoDBMessageQueueingService.DefaultCollectionName, databaseOverride);
        }

        [Fact]
        public async Task CollectionNameCanBeOverridden()
        {
            const string collectionOverride = "platibus.testQueue";
            GivenCollectionOverride(collectionOverride);
            await WhenMessageEnqueued();
            await AssertMessageDocumentInserted(collectionOverride);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("platibus.")]
        [InlineData("  platibus.  ")]
        public async Task CollectionPerQueueSupported(string prefix)
        {
            GivenCollectionPerQueue(prefix);
            await WhenMessageEnqueued();
            var expectedCollectionName = (prefix ?? "").Trim() + Queue;
            await AssertMessageDocumentInserted(expectedCollectionName);
        }
        
        public void GivenDatabaseOverride(string database)
        {
            Queueing.SetAttribute("database", database);
        }

        public void GivenCollectionOverride(string collection)
        {
            Queueing.SetAttribute("collection", collection);
        }

        public void GivenCollectionPerQueue(string prefix = null)
        {
            Queueing.SetAttribute("collectionPerQueue", "true");
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                Queueing.SetAttribute("collectionPrefix", prefix);
            }
        }

        public async Task WhenMessageEnqueued()
        {
            var messageQueueingService = await new MongoDBServicesProvider().CreateMessageQueueingService(Queueing);
            await messageQueueingService.CreateQueue(Queue, QueueListener);
            await messageQueueingService.EnqueueMessage(Queue, Message, null);
        }

        public async Task AssertMessageDocumentInserted(string collectionName, string database = null)
        {
            var db = MongoDBHelper.Connect(_connectionStringSettings, database);
            var coll = db.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("headers.Platibus-MessageId", Message.Headers.MessageId.ToString());
            var msg = await coll.Find(filter).FirstOrDefaultAsync();
            Assert.NotNull(msg);
        }
    }
}
