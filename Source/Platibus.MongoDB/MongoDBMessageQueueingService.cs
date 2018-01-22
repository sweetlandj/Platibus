// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Platibus.Queueing;
using Platibus.Security;
#if NET452
using System.Configuration;
#endif
#if NETSTANDARD2_0
using Platibus.Config;
#endif

namespace Platibus.MongoDB
{
    /// <summary>
    /// A <see cref="IMessageQueueingService"/> implementation that uses a MongoDB database to 
    /// store queued messages
    /// </summary>
    public class MongoDBMessageQueueingService : AbstractMessageQueueingService<MongoDBMessageQueue>
    {
        /// <summary>
        /// The default name of the collection that will be used to store queued messages
        /// </summary>
        public const string DefaultCollectionName = "platibus.queuedMessages";

        private readonly IMongoDatabase _database;
        private readonly ISecurityTokenService _securityTokenService;
        private readonly IMessageQueueingService _messageQueueingService;
        private readonly QueueCollectionNameFactory _collectionNameFactory;

        /// <summary>
        ///     Initializes a new <see cref="MongoDBMessageQueueingService"/>
        /// </summary>
        /// <param name="connectionStringSettings">
        ///     The connection string to use to connect to the MongoDB database
        /// </param>
        /// <param name="securityTokenService">
        ///     (Optional) The message security token  service to use to issue and validate
        ///     security tokens for persisted messages.
        /// </param>
        /// <param name="databaseName">
        ///     (Optional) The name of the database to use.  If omitted, the default database
        ///     identified in the <paramref name="connectionStringSettings"/> will be used
        /// </param>
        /// <param name="collectionNameFactory">
        ///     (Optional) A factory method used to generate a collection name corresponding 
        ///     to the specified queue.  The default is a single collection for all queues 
        ///     with a <see cref="DefaultCollectionName"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if  <paramref name="connectionStringSettings"/> is <c>null</c>
        /// </exception>
        public MongoDBMessageQueueingService(ConnectionStringSettings connectionStringSettings, 
            ISecurityTokenService securityTokenService = null, 
            string databaseName = null, QueueCollectionNameFactory collectionNameFactory = null)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException(nameof(connectionStringSettings));
            _database = MongoDBHelper.Connect(connectionStringSettings, databaseName);
            _securityTokenService = securityTokenService ?? new JwtSecurityTokenService();
            _collectionNameFactory = collectionNameFactory ?? (_ => DefaultCollectionName);
        }

        /// <inheritdoc />
        protected override Task<MongoDBMessageQueue> InternalCreateQueue(QueueName queueName, IQueueListener listener, 
            QueueOptions options = null, CancellationToken cancellationToken = new CancellationToken())
        {
            var collectionName = _collectionNameFactory(queueName);
            var queue = new MongoDBMessageQueue(_database, queueName, listener, _securityTokenService, options, collectionName);
            return Task.FromResult(queue);
        }
    }
}
