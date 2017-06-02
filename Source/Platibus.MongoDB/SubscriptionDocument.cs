using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Platibus.MongoDB
{
    internal class SubscriptionDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("topic")]
        public string Topic { get; set; }

        [BsonElement("subscriber")]
        public string Subscriber { get; set; }

        [BsonElement("expires")]
        public DateTime Expires { get; set; }
    }
}
