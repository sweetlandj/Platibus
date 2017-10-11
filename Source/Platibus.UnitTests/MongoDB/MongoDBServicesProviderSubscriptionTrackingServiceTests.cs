using MongoDB.Bson;
using MongoDB.Driver;
using Platibus.Config;
using Platibus.MongoDB;
using System;
using System.Configuration;
using System.Threading.Tasks;
using Xunit;

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
        public SubscriptionTrackingElement SubscriptionTracking = new SubscriptionTrackingElement();

        public MongoDBServicesProviderSubscriptionTrackingServiceTests(MongoDBFixture fixture)
        {
            _connectionStringSettings = fixture.ConnectionStringSettings;
            SubscriptionTracking.SetAttribute("connectionName", _connectionStringSettings.Name);
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
        
        public void GivenDatabaseOverride(string database)
        {
            SubscriptionTracking.SetAttribute("database", database);
        }

        public void GivenCollectionOverride(string collection)
        {
            SubscriptionTracking.SetAttribute("collection", collection);
        }
        
        public async Task WhenAddingASubscription()
        {
            var subscriptionTrackingService = await new MongoDBServicesProvider()
                .CreateSubscriptionTrackingService(SubscriptionTracking);

            await subscriptionTrackingService.AddSubscription(Topic, Subscriber);
        }

        public async Task AssertSubscriptionDocumentInserted(string collectionName, string database = null)
        {
            var db = MongoDBHelper.Connect(_connectionStringSettings, database);
            var coll = db.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("topic", Topic.ToString())
                & Builders<BsonDocument>.Filter.Eq("subscriber", Subscriber.ToString());
            var msg = await coll.Find(filter).FirstOrDefaultAsync();
            Assert.NotNull(msg);
        }
    }
}