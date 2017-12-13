using MongoDB.Bson;
using MongoDB.Driver;
using Platibus.Config;
using Platibus.MongoDB;
using System;
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
    public class MongoDBServicesProviderSubscriptionTrackingServiceTests
    {
        private readonly ConnectionStringSettings _connectionStringSettings;

        public TopicName Topic = Guid.NewGuid().ToString();
        public Uri Subscriber = new Uri("http://localhost/" + Guid.NewGuid());
#if NET452
        public SubscriptionTrackingElement Configuration = new SubscriptionTrackingElement();
#endif
#if NETCOREAPP2_0
        public IConfiguration Configuration;
#endif

        public MongoDBServicesProviderSubscriptionTrackingServiceTests(MongoDBFixture fixture)
        {
#if NETCOREAPP2_0

            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();
#endif
            _connectionStringSettings = fixture.ConnectionStringSettings;
            ConfigureAttribute("connectionName", _connectionStringSettings.Name);
        }

        [Fact]
        public async Task DatabaseNameCanBeOverridden()
        {
            const string databaseOverride = "pbtest1";
            GivenDatabaseOverride(databaseOverride);
            await WhenAddingASubscription();
            await AssertSubscriptionDocumentInserted(MongoDBSubscriptionTrackingService.DefaultCollectionName, databaseOverride);
        }

        [Fact]
        public async Task CollectionNameCanBeOverridden()
        {
            const string collectionOverride = "platibus.testSubscriptions";
            GivenCollectionOverride(collectionOverride);
            await WhenAddingASubscription();
            await AssertSubscriptionDocumentInserted(collectionOverride);
        }
        
        protected void GivenDatabaseOverride(string database)
        {
            ConfigureAttribute("database", database);
        }

        protected void GivenCollectionOverride(string collection)
        {
            ConfigureAttribute("collection", collection);
        }

        protected async Task WhenAddingASubscription()
        {
            var subscriptionTrackingService = await new MongoDBServicesProvider()
                .CreateSubscriptionTrackingService(Configuration);

            await subscriptionTrackingService.AddSubscription(Topic, Subscriber);
        }

        protected async Task AssertSubscriptionDocumentInserted(string collectionName, string database = null)
        {
            var db = MongoDBHelper.Connect(_connectionStringSettings, database);
            var coll = db.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("topic", Topic.ToString())
                & Builders<BsonDocument>.Filter.Eq("subscriber", Subscriber.ToString());
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