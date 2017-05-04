using System.Configuration;
using NUnit.Framework;
using Platibus.SQL;

namespace Platibus.UnitTests.LocalDB
{
    [SetUpFixture]
    public class LocalDBCollectionFixture
    {
        public static LocalDBCollectionFixture Instance;

        [SetUp]
        public void SetUp()
        {
            Instance = new LocalDBCollectionFixture();
            Instance.DeleteJournaledMessages();
            Instance.DeleteQueuedMessages();
        }

        [TearDown]
        public void TearDown()
        {
            if (Instance != null)
            {
                Instance._messageQueueingService.Dispose();
            }
        }

        private readonly IDbConnectionProvider _connectionProvider;
        private readonly ISQLDialect _dialect;
        private readonly SQLMessageJournalingService _messageJournalingService;
        private readonly SQLMessageQueueingService _messageQueueingService;

        public IDbConnectionProvider ConnectionProvider
        {
            get { return _connectionProvider; }
        }

        public ISQLDialect Dialect
        {
            get { return _dialect; }
        }

        public SQLMessageJournalingService MessageJournalingService
        {
            get { return _messageJournalingService; }
        }

        public SQLMessageQueueingService MessageQueueingService
        {
            get { return _messageQueueingService; }
        }

        public LocalDBCollectionFixture()
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["PlatibusUnitTests.LocalDB"];
            _connectionProvider = new DefaultConnectionProvider(connectionStringSettings);
            _dialect = new MSSQLDialect();

            _messageJournalingService = new SQLMessageJournalingService(_connectionProvider, _dialect);
            _messageJournalingService.Init();

            _messageQueueingService = new SQLMessageQueueingService(_connectionProvider, _dialect);
            _messageQueueingService.Init();
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
