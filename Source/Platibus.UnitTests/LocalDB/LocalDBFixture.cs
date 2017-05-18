using System;
using System.Configuration;
using Platibus.SQL;
using Platibus.SQL.Commands;

namespace Platibus.UnitTests.LocalDB
{
    public class LocalDBFixture : IDisposable
    {
        private readonly IDbConnectionProvider _connectionProvider;
        private readonly ISQLDialect _dialect;
        private readonly SQLMessageQueueingService _messageQueueingService;
        private readonly SQLSubscriptionTrackingService _subscriptionTrackingService;
        private readonly SQLMessageJournal _messageJournal;

        private bool _disposed;
        
        public IDbConnectionProvider ConnectionProvider
        {
            get { return _connectionProvider; }
        }

        public ISQLDialect Dialect
        {
            get { return _dialect; }
        }
        
        public SQLMessageJournal MessageJournal
        {
            get { return _messageJournal; }
        }

        public SQLMessageQueueingService MessageQueueingService
        {
            get { return _messageQueueingService; }
        }

        public SQLSubscriptionTrackingService SubscriptionTrackingService
        {
            get { return _subscriptionTrackingService; }
        }

        public LocalDBFixture()
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["PlatibusUnitTests.LocalDB"];
            _connectionProvider = new DefaultConnectionProvider(connectionStringSettings);
            _dialect = new MSSQLDialect();
            
            _messageJournal = new SQLMessageJournal(_connectionProvider, new MSSQLMessageJournalingCommandBuilders());
            _messageJournal.Init();

            _messageQueueingService = new SQLMessageQueueingService(_connectionProvider, _dialect);
            _messageQueueingService.Init();

            _subscriptionTrackingService = new SQLSubscriptionTrackingService(_connectionProvider, _dialect);
            _subscriptionTrackingService.Init();

            DeleteJournaledMessages();
            DeleteQueuedMessages();
            DeleteSubscriptions();
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_subscriptionTrackingService")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_messageQueueingService")]
        protected virtual void Dispose(bool disposing)
        {
            _messageQueueingService.TryDispose();
            _subscriptionTrackingService.TryDispose();
            _messageJournal.TryDispose();
        }

        public void DeleteQueuedMessages()
        {
            using (var connection = _connectionProvider.GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    IF (OBJECT_ID('[PB_QueuedMessages]')) IS NOT NULL 
                    BEGIN
                        DELETE FROM [PB_QueuedMessages]
                    END";

                command.ExecuteNonQuery();
            }
        }

        public void DeleteJournaledMessages()
        {
            using (var connection = _connectionProvider.GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    IF (OBJECT_ID('[PB_MessageJournal]')) IS NOT NULL 
                    BEGIN
                        DELETE FROM [PB_MessageJournal]
                    END";

                command.ExecuteNonQuery();
            }
        }

        public void DeleteSubscriptions()
        {
            using (var connection = _connectionProvider.GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    IF (OBJECT_ID('[PB_Subscriptions]')) IS NOT NULL 
                    BEGIN
                        DELETE FROM [PB_Subscriptions]
                    END";

                command.ExecuteNonQuery();
            }
        }
    }
}
