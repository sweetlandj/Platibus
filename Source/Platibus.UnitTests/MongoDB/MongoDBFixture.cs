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
using Platibus.Diagnostics;
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

        public IDiagnosticService DiagnosticService { get; } = new DiagnosticService();

        public ConnectionStringSettings ConnectionStringSettings { get; }

        public IMongoDatabase Database { get; }

        public MongoDBSubscriptionTrackingService SubscriptionTrackingService { get; }

        public MongoDBMessageQueueingService MessageQueueingService { get; }

        public MongoDBMessageJournal MessageJournal { get; }

        public MongoDBFixture()
        {
            var rng = new Random();
            DatabaseName = $"MongoDBFixture_{rng.Next(int.MaxValue):X}";

            // docker run -it --rm --name mongodb -p 27027:27017 mongo:3.6.0-jessie
            ConnectionStringSettings = new ConnectionStringSettings
            {
                Name = DatabaseName,
                ConnectionString = $"mongodb://localhost:27027/{DatabaseName}?maxpoolsize=1000"
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

            Database = MongoDBHelper.Connect(ConnectionStringSettings, DatabaseName);
            
            var subscriptionTrackingOptions = new MongoDBSubscriptionTrackingOptions(Database)
            {
                DiagnosticService = DiagnosticService
            };
            SubscriptionTrackingService = new MongoDBSubscriptionTrackingService(subscriptionTrackingOptions);

            var messageQueueingOptions = new MongoDBMessageQueueingOptions(Database)
            {
                DiagnosticService = DiagnosticService
            };
            MessageQueueingService = new MongoDBMessageQueueingService(messageQueueingOptions);

            var journalOptions = new MongoDBMessageJournalOptions(Database)
            {
                DiagnosticService = DiagnosticService
            };
            MessageJournal = new MongoDBMessageJournal(journalOptions);
        }

        public void DeleteJournaledMessages()
        {
            var journal = Database.GetCollection<BsonDocument>(MongoDBMessageJournal.DefaultCollectionName);
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
            if (Database != null && !string.IsNullOrWhiteSpace(DatabaseName))
            {
                try
                {
                    Database.Client.DropDatabase(DatabaseName);
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            if (disposing)
            {
                MessageQueueingService?.Dispose();
            }
        }
    }
}
