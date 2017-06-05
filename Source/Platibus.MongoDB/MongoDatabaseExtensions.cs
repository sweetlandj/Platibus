using MongoDB.Bson;
using MongoDB.Driver;

namespace Platibus.MongoDB
{
    internal static class MongoDatabaseExtensions
    {
        public static bool CollectionExists(this IMongoDatabase database, string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collections = database.ListCollections(new ListCollectionsOptions { Filter = filter });
            return collections.Any();
        }
    }
}
