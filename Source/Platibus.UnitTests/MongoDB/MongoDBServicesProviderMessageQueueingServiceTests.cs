using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Platibus.Config;
using Platibus.MongoDB;
using Platibus.Security;
using Platibus.UnitTests.Security;
using Xunit;
#if NET452
using System.Configuration;
#endif
#if NETCOREAPP2_0
using Microsoft.Extensions.Configuration;
#endif

namespace Platibus.UnitTests.MongoDB
{
    [Trait("Category", "UnitTests")]
    [Trait("Dependency", "MongoDB")]
    [Collection(MongoDBCollection.Name)]
    public class MongoDBServicesProviderMessageQueueingServiceTests
    {
        protected readonly ConnectionStringSettings ConnectionStringSettings;

        protected Message Message;
        protected QueueName Queue = Guid.NewGuid().ToString();
        protected IQueueListener QueueListener = new Mock<IQueueListener>().Object;

#if NET452
        public QueueingElement Configuration = new QueueingElement();
#endif
#if NETCOREAPP2_0
        public IConfiguration Configuration;
#endif

        public MongoDBServicesProviderMessageQueueingServiceTests(MongoDBFixture fixture)
        {
#if NETCOREAPP2_0
            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();
#endif
            ConnectionStringSettings = fixture.ConnectionStringSettings;
#if NET452
            Configuration.SetAttribute("connectionName", fixture.ConnectionStringSettings.Name);
#endif
#if NETCOREAPP2_0
            Configuration["connectionName"] = fixture.ConnectionStringSettings.Name;
#endif

            Message = new Message(new MessageHeaders
            {
                MessageId = MessageId.Generate()
            }, "MongoDBServicesProviderMessageQueueingServiceTests");
        }

        [Fact]
        public async Task DatabaseNameCanBeOverridden()
        {
            var databaseOverride = ConnectionStringSettings.Name + "_override";
            GivenDatabaseOverride(databaseOverride);
            await WhenMessageEnqueued();
            await AssertMessageDocumentInserted(MongoDBMessageQueueingService.DefaultCollectionName, databaseOverride);
        }

        [Fact]
        public async Task CollectionNameCanBeOverridden()
        {
            const string collectionOverride = MongoDBMessageQueueingService.DefaultCollectionName + "_override";
            GivenCollectionOverride(collectionOverride);
            await WhenMessageEnqueued();
            await AssertMessageDocumentInserted(collectionOverride);
        }

        [Fact]
        public async Task MessagesCanBeEncrypted()
        {
            GivenEncryption();
            await WhenMessageEnqueued();
            await AssertEncryptedMessageDocumentInserted(MongoDBMessageQueueingService.DefaultCollectionName);
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
        
        protected void GivenDatabaseOverride(string database)
        {
            ConfigureAttribute("database", database);
        }

        protected void GivenCollectionOverride(string collection)
        {
            ConfigureAttribute("collection", collection);
        }

        protected void GivenCollectionPerQueue(string prefix = null)
        {
            ConfigureAttribute("collectionPerQueue", "true");
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                ConfigureAttribute("collectionPrefix", prefix);
            }
        }

        protected void GivenEncryption()
        {
#if NET452
            Configuration.Encryption = new EncryptionElement
            {
                Enabled = true,
                Provider = "AES",
                Key = HexEncoding.GetString(KeyGenerator.GenerateAesKey().Key)
            };
#endif
#if NETCOREAPP2_0
            var section = Configuration.GetSection("encryption");
            section["enabled"] = "true";
            section["provider"] = "aes";
            section["key"] = HexEncoding.GetString(KeyGenerator.GenerateAesKey().Key);
#endif
        }

        protected async Task WhenMessageEnqueued()
        {
            var messageQueueingService = await new MongoDBServicesProvider().CreateMessageQueueingService(Configuration);
            await messageQueueingService.CreateQueue(Queue, QueueListener);
            await messageQueueingService.EnqueueMessage(Queue, Message, null);
        }

        protected async Task AssertMessageDocumentInserted(string collectionName, string database = null)
        {
            var db = MongoDBHelper.Connect(ConnectionStringSettings, database);
            var coll = db.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("headers.Platibus-MessageId", Message.Headers.MessageId.ToString());
            var msg = await coll.Find(filter).FirstOrDefaultAsync();
            Assert.NotNull(msg);
        }

        protected async Task AssertEncryptedMessageDocumentInserted(string collectionName, string database = null)
        {
            var db = MongoDBHelper.Connect(ConnectionStringSettings, database);
            var coll = db.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("headers.Platibus-MessageId", Message.Headers.MessageId.ToString());
            var journalEntry = await coll.Find(filter).FirstOrDefaultAsync();
            Assert.NotNull(journalEntry);

            var headers = ReadHeaders(journalEntry);
            Assert.NotNull(headers.IV);
            Assert.NotNull(headers.Signature);
        }

        private static EncryptedMessageHeaders ReadHeaders(BsonDocument journalEntry)
        {
            var headersSubdocument = journalEntry.GetElement("headers").Value.ToBsonDocument();
            var headersDict = headersSubdocument.ToDictionary(
                element => element.Name, 
                element => element.Value.AsString);

           return new EncryptedMessageHeaders(headersDict);
        }

        protected void ConfigureAttribute(string name, string value)
        {
#if NET452
            Configuration.SetAttribute(name, value);
#endif
#if NETCOREAPP2_0
            Configuration[name] = value;
#endif
        }
    }
}
