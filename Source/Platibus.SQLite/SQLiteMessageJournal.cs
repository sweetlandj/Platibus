using System;
using System.Configuration;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Platibus.Journaling;
using Platibus.SQL;
using Platibus.SQLite.Commands;

namespace Platibus.SQLite
{
    /// <summary>
    /// An <see cref="IMessageQueueingService"/> implementation that stores queued
    /// messages in a SQLite database
    /// </summary>
    public class SQLiteMessageJournal : SQLMessageJournal, IDisposable
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
        public SQLiteMessageJournal(DirectoryInfo baseDirectory)
            : base(InitConnectionProvider(baseDirectory), new SQLiteMessageJournalCommandBuilders())
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

        private static IDbConnectionProvider InitConnectionProvider(DirectoryInfo directory)
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
                ConnectionString = "Data Source=" + dbPath + "; Version=3; BinaryGUID=False",
                ProviderName = "System.Data.SQLite"
            };

            return new SingletonConnectionProvider(connectionStringSettings);
        }

        /// <inheritdoc />
        public override async Task Append(Message message, JournaledMessageCategory category,
            CancellationToken cancellationToken = new CancellationToken())
        {
            CheckDisposed();
            var op = new SQLiteOperation(() => base.Append(message, category, cancellationToken));
            await _operationQueue.SendAsync(op, cancellationToken);
            await op.Task;
        }

        /// <inheritdoc />
        public override async Task<MessageJournalReadResult> Read(MessageJournalPosition start, int count, MessageJournalFilter filter = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            CheckDisposed();
            var op = new SQLiteOperation<MessageJournalReadResult>(() => base.Read(start, count, filter, cancellationToken));
            await _operationQueue.SendAsync(op, cancellationToken);
            return await op.Task;
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
        ~SQLiteMessageJournal()
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