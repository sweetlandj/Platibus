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

        private readonly IDbConnectionProvider _connectionProvider;
        private readonly ISQLDialect _dialect;

        public IDbConnectionProvider ConnectionProvider { get { return _connectionProvider; } }
        public ISQLDialect Dialect { get { return _dialect; } }

        public LocalDBCollectionFixture()
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["PlatibusUnitTests.LocalDB"];
            _connectionProvider = new DefaultConnectionProvider(connectionStringSettings);
            _dialect = new MSSQLDialect();
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
