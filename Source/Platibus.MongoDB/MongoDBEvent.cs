using System;
using Platibus.Diagnostics;

namespace Platibus.MongoDB
{
    /// <summary>
    /// A diagnostic event related to MongoDB integration
    /// </summary>
    public class MongoDBEvent : DiagnosticEvent
    {
        private readonly string _databaseName;
        private readonly string _collectionName;
        private readonly string _indexName;

        /// <summary>
        /// The name of the MongoDB database
        /// </summary>
        public string DatabaseName { get { return _databaseName; } }

        /// <summary>
        /// The name of the MongoDB collection
        /// </summary>
        public string CollectionName { get { return _collectionName; } }

        /// <summary>
        /// The name of the MongoDB index
        /// </summary>
        public string IndexName { get { return _indexName; } }

        /// <summary>
        /// Initializes a new <see cref="MongoDBEvent"/>
        /// </summary>
        /// <param name="source">The object that emitted the event</param>
        /// <param name="type">The type of event</param>
        /// <param name="detail">Specific details regarding this instance of the event</param>
        /// <param name="exception">The exception related to the event, if applicable</param>
        /// <param name="message">The message to which the event pertains, if applicable</param>
        /// <param name="endpoint">The name of the endpoint, if applicable</param>
        /// <param name="queue">The queue to which the event pertains, if applicable</param>
        /// <param name="topic">The topic to which the message pertains, if applicable</param>
        /// <param name="databaseName">The name of the MongoDB database, if applicable</param>
        /// <param name="collectionName">The name of the MongoDB collection, if applicable</param>
        /// <param name="indexName">The name of the MongoDB index, if applicable</param>
        public MongoDBEvent(object source, DiagnosticEventType type, string detail = null, Exception exception = null, Message message = null, EndpointName endpoint = null, QueueName queue = null, TopicName topic = null, string databaseName = null, string collectionName = null, string indexName = null) 
            : base(source, type, detail, exception, message, endpoint, queue, topic)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _indexName = indexName;
        }
    }
}
