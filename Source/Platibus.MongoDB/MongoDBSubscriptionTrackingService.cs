// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Platibus.MongoDB
{
    /// <summary>
    /// An implementation of <see cref="ISubscriptionTrackingService"/> that stores subscriptions
    /// in a MongoDB database
    /// </summary>
    public class MongoDBSubscriptionTrackingService : ISubscriptionTrackingService
    {
        /// <summary>
        /// The default name of the collection that will be used to store subscription information
        /// </summary>
        public const string DefaultCollectionName = "platibus.subscriptions";

        private readonly IMongoCollection<SubscriptionDocument> _subscriptions;

        /// <summary>
        /// Initializes a new <see cref="MongoDBSubscriptionTrackingService"/> with the specified
        /// <paramref name="connectionStringSettings"/> and <paramref name="databaseName"/>
        /// </summary>
        /// <param name="connectionStringSettings">The connection string to use to connect to the
        /// MongoDB database</param>
        /// <param name="databaseName">(Optional) The name of the database to use.  If omitted,
        /// the default database identified in the <paramref name="connectionStringSettings"/>
        /// will be used</param>
        /// <param name="collectionName">(Optional) The name of the collection in which 
        /// subscription documents will be stored.  If omitted, the
        /// <see cref="DefaultCollectionName"/> will be used</param>
        public MongoDBSubscriptionTrackingService(ConnectionStringSettings connectionStringSettings, string databaseName = null, string collectionName = DefaultCollectionName)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException("connectionStringSettings");
            var mongoUrl = new MongoUrl(connectionStringSettings.ConnectionString);
            var myDatabaseName = string.IsNullOrWhiteSpace(databaseName)
                ? mongoUrl.DatabaseName
                : databaseName;

            var myCollectionName = string.IsNullOrWhiteSpace(collectionName)
                ? DefaultCollectionName
                : collectionName;

            var client = new MongoClient(mongoUrl);
            var database = client.GetDatabase(myDatabaseName);
            _subscriptions = database.GetCollection<SubscriptionDocument>(myCollectionName);
        }

        /// <inheritdoc />
        public Task AddSubscription(TopicName topic, Uri subscriber, TimeSpan ttl = new TimeSpan(),
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (topic == null) throw new ArgumentNullException("topic");
            if (subscriber == null) throw new ArgumentNullException("subscriber");

            var expires = ttl <= TimeSpan.Zero
                ? DateTime.MaxValue
                : DateTime.UtcNow.Add(ttl);

            var fb = Builders<SubscriptionDocument>.Filter;
            var filter = fb.Eq(s => s.Topic, topic.ToString()) &
                         fb.Eq(s => s.Subscriber, subscriber.ToString());

            var update = Builders<SubscriptionDocument>.Update
                .Set(s => s.Expires, expires);

            return _subscriptions.UpdateOneAsync(filter, update, new UpdateOptions {IsUpsert = true}, cancellationToken);
        }

        /// <inheritdoc />
        public Task RemoveSubscription(TopicName topic, Uri subscriber, CancellationToken cancellationToken = new CancellationToken())
        {
            if (topic == null) throw new ArgumentNullException("topic");
            if (subscriber == null) throw new ArgumentNullException("subscriber");

            var fb = Builders<SubscriptionDocument>.Filter;
            var filter = fb.Eq(s => s.Topic, topic.ToString()) &
                         fb.Eq(s => s.Subscriber, subscriber.ToString());

            return _subscriptions.DeleteManyAsync(filter, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Uri>> GetSubscribers(TopicName topic, CancellationToken cancellationToken = new CancellationToken())
        {
            var fb = Builders<SubscriptionDocument>.Filter;
            var filter = fb.Eq(s => s.Topic, topic.ToString()) &
                         fb.Gt(s => s.Expires, DateTime.UtcNow);                

            var subscrptionDocuments = await _subscriptions.Find(filter).ToListAsync(cancellationToken);
            return subscrptionDocuments.Select(s => new Uri(s.Subscriber));
        }
    }
}
