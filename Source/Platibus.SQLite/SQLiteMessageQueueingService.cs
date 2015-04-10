using Common.Logging;
using Platibus.SQL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Platibus.SQLite
{
    public class SQLiteMessageQueueingService : SQLMessageQueueingService, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(SQLiteLoggingCategories.SQLite);

        private readonly BufferBlock<Task> _queuedOperations;
        private readonly CancellationTokenSource _cancellationTokenSource;
        
        private Task _sqliteBackgroundWorker;
        private DbConnection _connection;
        private bool _disposed;

        public SQLiteMessageQueueingService(ConnectionStringSettings connectionStringSettings)
            : base(connectionStringSettings)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _queuedOperations = new BufferBlock<Task>(new DataflowBlockOptions
            {
                CancellationToken = _cancellationTokenSource.Token
            });
        }

        public override void Init()
        {
            base.Init();
            _connection = OpenConnection();
            _sqliteBackgroundWorker = ProcessOperations();
        }

        public override async Task CreateQueue(QueueName queueName, IQueueListener listener, QueueOptions options = default(QueueOptions))
        {
            CheckDisposed();
            var queue = new SQLMessageQueue(OpenConnection, Dialect, queueName, listener, options);
            if (!Queues.TryAdd(queueName, queue))
            {
                throw new QueueAlreadyExistsException(queueName);
            }

            Log.DebugFormat("Initializing SQLite queue named \"{0}\"...", queueName);
            var initTask = new Task(async () =>
            {
                await queue.Init().ConfigureAwait(false);
            });
            await _queuedOperations.SendAsync(initTask);
            await initTask;
            Log.DebugFormat("SQLite queue \"{0}\" created successfully", queueName);
        }

        public override async Task EnqueueMessage(QueueName queueName, Message message, IPrincipal senderPrincipal)
        {
            CheckDisposed();
            var enqueueTask = new Task(() =>
            {
                base.EnqueueMessage(queueName, message, senderPrincipal);
            });
            await _queuedOperations.SendAsync(enqueueTask);
            await enqueueTask;
        }

        private async Task ProcessOperations(CancellationToken cancellationToken = default(CancellationToken))
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var nextOperation = await _queuedOperations.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                nextOperation.Start();
            }
        }

        protected override DbConnection OpenConnection()
        {
            return _connection ?? ConnectionStringSettings.OpenConnection();
        }

        ~SQLiteMessageQueueingService()
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

            _cancellationTokenSource.Cancel();
            _queuedOperations.Complete();

            try
            {
                _connection.Close();
            }
            catch (Exception ex)
            { 
            }
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
