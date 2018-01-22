using System;
#if NET452
using System.Configuration;
#endif
#if NETCOREAPP2_0
using Platibus.Config;
#endif
using Platibus.Security;
using Platibus.SQL;
using Platibus.SQL.Commands;
using Platibus.UnitTests.Security;

namespace Platibus.UnitTests.LocalDB
{
    public class AesEncryptedLocalDBFixture : IDisposable
    {
        private bool _disposed;
        
        public IDbConnectionProvider ConnectionProvider { get; }

        public AesMessageEncryptionService MessageEncryptionService { get; }

        public SQLMessageQueueingService MessageQueueingService { get; }

        public AesEncryptedLocalDBFixture()
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["PlatibusUnitTests.LocalDB"];
            ConnectionProvider = new DefaultConnectionProvider(connectionStringSettings);

            var aesOptions = new AesMessageEncryptionOptions(KeyGenerator.GenerateAesKey());
            MessageEncryptionService = new AesMessageEncryptionService(aesOptions);
            
            var queueingOptions = new SQLMessageQueueingOptions(ConnectionProvider, new MSSQLMessageQueueingCommandBuilders())
            {
                MessageEncryptionService = MessageEncryptionService
            };
            MessageQueueingService = new SQLMessageQueueingService(queueingOptions);
            MessageQueueingService.Init();

            DeleteQueuedMessages();
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
            MessageQueueingService.Dispose();
        }

        public void DeleteQueuedMessages()
        {
            using (var connection = ConnectionProvider.GetConnection())
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
        
    }
}