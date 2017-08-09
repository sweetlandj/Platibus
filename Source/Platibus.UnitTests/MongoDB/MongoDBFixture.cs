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
using System.Configuration;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Driver;
using Platibus.MongoDB;

namespace Platibus.UnitTests.MongoDB
{
    public class MongoDBFixture : IDisposable
    {
        public const string DatabaseName = "platibus_UnitTests";

        private readonly MongoDbRunner _mongoDbRunner;
        private readonly ConnectionStringSettings _connectionStringSettings;
        
        private readonly MongoDBSubscriptionTrackingService _subscriptionTrackingService;
        private readonly MongoDBMessageQueueingService _messageQueueingService;
        private readonly MongoDBMessageJournal _messageJournal;

        private bool _disposed;

        public ConnectionStringSettings ConnectionStringSettings
        {
            get { return _connectionStringSettings; }
        }

        public MongoDBSubscriptionTrackingService SubscriptionTrackingService
        {
            get { return _subscriptionTrackingService; }
        }

        public MongoDBMessageQueueingService MessageQueueingService
        {
            get { return _messageQueueingService; }
        }

        public MongoDBMessageJournal MessageJournal
        {
            get { return _messageJournal; }
        }

        public MongoDBFixture()
        {
            var dbPath = FileUtil.NewTempTestPath();
            _mongoDbRunner = MongoDbRunner.Start(dbPath);

            _connectionStringSettings = new ConnectionStringSettings
            {
                ConnectionString = _mongoDbRunner.ConnectionString + "?maxpoolsize=1000"
            };
            
            _subscriptionTrackingService = new MongoDBSubscriptionTrackingService(_connectionStringSettings, DatabaseName);
            _messageQueueingService = new MongoDBMessageQueueingService(_connectionStringSettings, databaseName: DatabaseName);
            _messageJournal = new MongoDBMessageJournal(_connectionStringSettings, DatabaseName);
        }

        public void DeleteJournaledMessages()
        {
            var db = MongoDBHelper.Connect(_connectionStringSettings, DatabaseName);
            var journal = db.GetCollection<BsonDocument>(MongoDBMessageJournal.DefaultCollectionName);
            journal.DeleteMany(Builders<BsonDocument>.Filter.Empty);
        }

        ~MongoDBFixture()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _messageQueueingService.Dispose();
            }
            _mongoDbRunner.Dispose();
        }
    }
}
