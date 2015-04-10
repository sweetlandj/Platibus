﻿using Common.Logging;
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
    public class SQLMessageQueueingService : IMessageQueueingService, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.SQL);

        private readonly IDbConnectionProvider _connectionProvider;
        private readonly ISQLDialect _dialect;

        protected readonly ConcurrentDictionary<QueueName, SQLMessageQueue> Queues = new ConcurrentDictionary<QueueName, SQLMessageQueue>();
        
        private bool _disposed;

        public IDbConnectionProvider ConnectionProvider
        {
            get { return _connectionProvider; }
        }

        public ISQLDialect Dialect
        {
            get { return _dialect; }
        }

        public SQLMessageQueueingService(ConnectionStringSettings connectionStringSettings, ISQLDialect dialect = null)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException("connectionStringSettings");
            _connectionProvider = new DefaultConnectionProvider(connectionStringSettings);
            _dialect = dialect ?? connectionStringSettings.GetSQLDialect();
        }

        public SQLMessageQueueingService(IDbConnectionProvider connectionProvider, ISQLDialect dialect)
        {
            if (connectionProvider == null) throw new ArgumentNullException("connectionProvider");
            if (dialect == null) throw new ArgumentNullException("dialect");
            _connectionProvider = connectionProvider;
            _dialect = dialect;
        }
        
        public virtual void Init()
        {
            var connection = _connectionProvider.GetConnection();
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = _dialect.CreateObjectsCommand;
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                _connectionProvider.ReleaseConnection(connection);
            }
        }

        public virtual async Task CreateQueue(QueueName queueName, IQueueListener listener, QueueOptions options = default(QueueOptions))
        {
            var queue = new SQLMessageQueue(_connectionProvider, _dialect, queueName, listener, options);
            if (!Queues.TryAdd(queueName, queue))
            {
                throw new QueueAlreadyExistsException(queueName);
            }

            Log.DebugFormat("Initializing SQL queue named \"{0}\"...", queueName);
            await queue.Init().ConfigureAwait(false);
            Log.DebugFormat("SQL queue \"{0}\" created successfully", queueName);
        }

        public virtual async Task EnqueueMessage(QueueName queueName, Message message, IPrincipal senderPrincipal)
        {
            SQLMessageQueue queue;
            if (!Queues.TryGetValue(queueName, out queue)) throw new QueueNotFoundException(queueName);

            Log.DebugFormat("Enqueueing message ID {0} in SQL queue \"{1}\"...", message.Headers.MessageId, queueName);
            await queue.Enqueue(message, senderPrincipal).ConfigureAwait(false);
            Log.DebugFormat("Message ID {0} enqueued successfully in SQL queue \"{1}\"", message.Headers.MessageId, queueName);
        }

        ~SQLMessageQueueingService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _connectionProvider.Dispose();
        }

        protected void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }
}