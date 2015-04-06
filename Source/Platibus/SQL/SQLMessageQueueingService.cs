using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.SQL
{
    public class SQLMessageQueueingService : IMessageQueueingService
    {
        private readonly ConnectionStringSettings _connectionStringSettings;
        private readonly ISQLDialect _dialect;

        protected ConnectionStringSettings ConnectionStringSettings
        {
            get { return _connectionStringSettings; }
        }

        public SQLMessageQueueingService(ConnectionStringSettings connectionStringSettings, ISQLDialect dialect = null)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException("connectionStringSettings");
            _connectionStringSettings = connectionStringSettings;
            _dialect = dialect ?? _connectionStringSettings.GetSQLDialect();
        }

        public void Init()
        {
            using (var connection = _connectionStringSettings.OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = _dialect.CreateObjectsCommand;
                command.ExecuteNonQuery();
            }
        }

        public Task CreateQueue(QueueName queueName, IQueueListener listener, QueueOptions options = default(QueueOptions))
        {
            throw new NotImplementedException();
        }

        public Task EnqueueMessage(QueueName queueName, Message message, IPrincipal senderPrincipal)
        {
            throw new NotImplementedException();
        }
    }
}
