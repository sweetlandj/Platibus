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
using MongoDB.Bson;
using MongoDB.Driver;
using Platibus.MongoDB;
#if NET452
using System.Configuration;
#endif
#if NETCOREAPP2_0
using Platibus.Config;
#endif

namespace Platibus.UnitTests.MongoDB
{
    public class MongoDBFixture : IDisposable
    {
        private bool _disposed;

        public string DatabaseName { get; }

        public ConnectionStringSettings ConnectionStringSettings { get; }

        public MongoDBSubscriptionTrackingService SubscriptionTrackingService { get; }

        public MongoDBMessageQueueingService MessageQueueingService { get; }

        public MongoDBMessageJournal MessageJournal { get; }

        public MongoDBFixture()
        {
            var rng = new Random();
            DatabaseName = $"platibus{rng.Next(int.MaxValue):X}";

            // docker run -it --rm --name mongodb -p 27017:27017 mongo:3.6.0-jessie
            ConnectionStringSettings = new ConnectionStringSettings
            {
                Name = "MongoDBFixture",
                ConnectionString = $"mongodb://localhost:27017/{DatabaseName}?maxpoolsize=1000"
            };
#if NET452

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.ConnectionStrings.ConnectionStrings.Remove(ConnectionStringSettings.Name);
            config.ConnectionStrings.ConnectionStrings.Add(ConnectionStringSettings);
            config.Save();
            ConfigurationManager.RefreshSection("connectionStrings");
#endif
#if NETCOREAPP2_0
            ConfigurationManager.ConnectionStrings[ConnectionStringSettings.Name] = ConnectionStringSettings;
#endif
            
            SubscriptionTrackingService = new MongoDBSubscriptionTrackingService(ConnectionStringSettings, DatabaseName);
            MessageQueueingService = new MongoDBMessageQueueingService(ConnectionStringSettings, databaseName: DatabaseName);
            MessageJournal = new MongoDBMessageJournal(ConnectionStringSettings, DatabaseName);
        }

        public void DeleteJournaledMessages()
        {
            var db = MongoDBHelper.Connect(ConnectionStringSettings, DatabaseName);
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
            if (ConnectionStringSettings != null && !string.IsNullOrWhiteSpace(DatabaseName))
            {
                try
                {
                    var db = MongoDBHelper.Connect(ConnectionStringSettings, DatabaseName);
                    db.Client.DropDatabase(DatabaseName);
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            if (disposing)
            {
                if (MessageQueueingService != null)
                {
                    MessageQueueingService.Dispose();
                }
            }
        }
    }
}
