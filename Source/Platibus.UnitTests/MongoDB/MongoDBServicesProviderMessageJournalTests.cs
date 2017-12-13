using MongoDB.Bson;
using MongoDB.Driver;
using Platibus.Config;
using Platibus.Journaling;
using Platibus.MongoDB;
using System.Threading.Tasks;
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
    public class MongoDBServicesProviderMessageJournalTests
    {
        private readonly ConnectionStringSettings _connectionStringSettings;

        public Message Message;
#if NET452
        public JournalingElement Configuration = new JournalingElement();
#endif
#if NETCOREAPP2_0
        public IConfiguration Configuration;
#endif

        public MongoDBServicesProviderMessageJournalTests(MongoDBFixture fixture)
        {
#if NETCOREAPP2_0

            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();
#endif
            _connectionStringSettings = fixture.ConnectionStringSettings;
            ConfigureAttribute("connectionName", _connectionStringSettings.Name);

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
        
        protected void GivenDatabaseOverride(string database)
        {
            ConfigureAttribute("database", database);
        }

        protected void GivenCollectionOverride(string collection)
        {
            ConfigureAttribute("collection", collection);
        }

        protected async Task WhenMessageJournaled()
        {
            var messageJournal = await new MongoDBServicesProvider().CreateMessageJournal(Configuration);
            await messageJournal.Append(Message, MessageJournalCategory.Sent);
        }

        protected async Task AssertMessageDocumentInserted(string collectionName, string database = null)
        {
            var db = MongoDBHelper.Connect(_connectionStringSettings, database);
            var coll = db.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("headers.Platibus-MessageId", Message.Headers.MessageId.ToString());
            var msg = await coll.Find(filter).FirstOrDefaultAsync();
            Assert.NotNull(msg);
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