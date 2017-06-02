using System;
using System.Configuration;
using Mongo2Go;
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

        public MongoDBFixture()
        {
            var dbPath = FileUtil.NewTempTestPath();
            _mongoDbRunner = MongoDbRunner.Start(dbPath);

            _connectionStringSettings = new ConnectionStringSettings
            {
                ConnectionString = _mongoDbRunner.ConnectionString
            };
            
            _subscriptionTrackingService = new MongoDBSubscriptionTrackingService(_connectionStringSettings, DatabaseName);
            _messageQueueingService = new MongoDBMessageQueueingService(_connectionStringSettings, databaseName: DatabaseName);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_subscriptionTrackingService")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_runner")]
        protected virtual void Dispose(bool disposing)
        {
            _subscriptionTrackingService.TryDispose();
            _messageQueueingService.TryDispose();
            _mongoDbRunner.TryDispose();
        }
    }
}
