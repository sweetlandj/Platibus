using Common.Logging;
using System;
using System.Collections.Concurrent;
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
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.SQL);

        private readonly ConnectionStringSettings _connectionStringSettings;
        private readonly ISQLDialect _dialect;
        private readonly ConcurrentDictionary<QueueName, SQLMessageQueue> _queues = new ConcurrentDictionary<QueueName, SQLMessageQueue>();

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

        public async Task CreateQueue(QueueName queueName, IQueueListener listener, QueueOptions options = default(QueueOptions))
        {
            var queue = new SQLMessageQueue(_connectionStringSettings, _dialect, queueName, listener, options);
            if (!_queues.TryAdd(queueName, queue))
            {
                throw new QueueAlreadyExistsException(queueName);
            }

            Log.DebugFormat("Initializing SQL queue named \"{0}\"...", queueName);
            await queue.Init().ConfigureAwait(false);
            Log.DebugFormat("SQL queue \"{0}\" created successfully", queueName);
        }

        public async Task EnqueueMessage(QueueName queueName, Message message, IPrincipal senderPrincipal)
        {
            SQLMessageQueue queue;
            if (!_queues.TryGetValue(queueName, out queue)) throw new QueueNotFoundException(queueName);

            Log.DebugFormat("Enqueueing message ID {0} in SQL queue \"{1}\"...", message.Headers.MessageId, queueName);
            await queue.Enqueue(message, senderPrincipal).ConfigureAwait(false);
            Log.DebugFormat("Message ID {0} enqueued successfully in SQL queue \"{1}\"", message.Headers.MessageId, queueName);
        }
    }
}
