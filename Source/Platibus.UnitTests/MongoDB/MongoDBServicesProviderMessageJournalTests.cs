using MongoDB.Bson;
using MongoDB.Driver;
using Platibus.Config;
using Platibus.Journaling;
using Platibus.MongoDB;
using System.Configuration;
using System.Threading.Tasks;
using Xunit;

namespace Platibus.UnitTests.MongoDB
{
    [Collection(MongoDBCollection.Name)]
    public class MongoDBServicesProviderMessageJournalTests
    {
        private readonly ConnectionStringSettings _connectionStringSettings;

        public Message Message;
        public JournalingElement Journaling = new JournalingElement();

        public MongoDBServicesProviderMessageJournalTests(MongoDBFixture fixture)
        {
            _connectionStringSettings = fixture.ConnectionStringSettings;
            Journaling.SetAttribute("connectionName", _connectionStringSettings.Name);

            Message = new Message(new MessageHeaders
            {
                MessageId = MessageId.Generate()
            }, "MongoDBServicesProviderMessageJournalTests");
        }

        [Fact]
        public async Task DatabaseNameCanBeOverridden()
        {
            const string databaseOverride = "pbtest1";
            GivenDatabaseOverride(databaseOverride);
            await WhenMessageJournaled();
            await AssertMessageDocumentInserted(MongoDBMessageJournal.DefaultCollectionName, databaseOverride);
        }

        [Fact]
        public async Task CollectionNameCanBeOverridden()
        {
            const string collectionOverride = "platibus.testJournal";
            GivenCollectionOverride(collectionOverride);
            await WhenMessageJournaled();
            await AssertMessageDocumentInserted(collectionOverride);
        }
        
        public void GivenDatabaseOverride(string database)
        {
            Journaling.SetAttribute("database", database);
        }

        public void GivenCollectionOverride(string collection)
        {
            Journaling.SetAttribute("collection", collection);
        }
        
        public async Task WhenMessageJournaled()
        {
            var messageJournal = await new MongoDBServicesProvider().CreateMessageJournal(Journaling);
            await messageJournal.Append(Message, MessageJournalCategory.Sent);
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