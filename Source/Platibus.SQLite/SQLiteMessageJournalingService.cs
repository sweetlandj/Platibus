using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Platibus.SQL;

namespace Platibus.SQLite
{
    /// <summary>
    /// An <see cref="IMessageQueueingService"/> implementation that stores queued
    /// messages in a SQLite database
    /// </summary>
    public class SQLiteMessageJournalingService : SQLMessageJournalingService, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ActionBlock<ISQLiteOperation> _operationQueue;

        private bool _disposed;

        /// <summary>
        /// Initializes a new <see cref="SQLiteMessageQueueingService"/>
        /// </summary>
        /// <param name="baseDirectory">The directory in which the SQLite database files will
        /// be created</param>
        /// <remarks>
        /// If a base directory is not specified then the base directory will default to a
        /// directory named <c>platibus\queues</c> beneath the current app domain base 
        /// directory.  If the base directory does not exist it will be created.
        /// </remarks>
        public SQLiteMessageJournalingService(DirectoryInfo baseDirectory)
            : base(InitDb(baseDirectory), new SQLiteDialect())
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _operationQueue = new ActionBlock<ISQLiteOperation>(
                op => op.Execute(),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellationTokenSource.Token,
                    MaxDegreeOfParallelism = 1
                });
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private static IDbConnectionProvider InitDb(DirectoryInfo directory)
        {
            if (directory == null)
            {
                var appdomainDirectory = AppDomain.CurrentDomain.BaseDirectory;
                directory = new DirectoryInfo(Path.Combine(appdomainDirectory, "platibus", "journal"));
            }

            var dbPath = Path.Combine(directory.FullName, "journal.db");
            var connectionStringSettings = new ConnectionStringSettings
            {
                Name = dbPath,
                ConnectionString = "Data Source=" + dbPath + "; Version=3",
                ProviderName = "System.Data.SQLite"
            };

            var connectionProvider = new SingletonConnectionProvider(connectionStringSettings);
            var connection = connectionProvider.GetConnection();
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = new SQLiteDialect().CreateMessageJournalingServiceObjectsCommand;
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                connectionProvider.ReleaseConnection(connection);
            }
            return connectionProvider;
        }

        /// <inheritdoc />
        protected override Task<SQLJournaledMessage> InsertJournaledMessage(Message message, string category, DateTimeOffset timestamp = default(DateTimeOffset))
        {
            CheckDisposed();
            var op = new SQLiteOperation<SQLJournaledMessage>(() => base.InsertJournaledMessage(message, category, timestamp));
            _operationQueue.Post(op);
            return op.Task;
        }

        /// <inheritdoc />
        protected override Task<IEnumerable<SQLJournaledMessage>> SelectJournaledMessages()
        {
            CheckDisposed();
            var op = new SQLiteOperation<IEnumerable<SQLJournaledMessage>>(() => base.SelectJournaledMessages());
            _operationQueue.Post(op);
            return op.Task;
        }

        /// <summary>
        /// Throws <see cref="ObjectDisposedException"/> if this object has been disposed
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if this object has been disposed</exception>
        protected virtual void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        /// <summary>
        /// Finalizer to ensure that all resources are released
        /// </summary>
        ~SQLiteMessageJournalingService()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called by the <see cref="Dispose()"/> method or by the finalizer to free held resources
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the 
        /// <see cref="Dispose()"/> method (<c>true</c>) or from the finalizer (<c>false</c>)</param>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_cancellationTokenSource")]
        protected virtual void Dispose(bool disposing)
        {
            _cancellationTokenSource.Cancel();
            if (disposing)
            {
                _cancellationTokenSource.TryDispose();
            }
        }
    }
}
