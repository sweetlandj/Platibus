using Platibus.SQL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Platibus.SQLite
{
    class SQLiteMessageQueue : SQLMessageQueue
    {
        private readonly ActionBlock<ISQLiteOperation> _operationQueue;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public SQLiteMessageQueue(DirectoryInfo baseDirectory, QueueName queueName, IQueueListener listener, QueueOptions options = default(QueueOptions))
            : base(InitDb(baseDirectory, queueName), new SQLiteDialect(), queueName, listener, options)
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

        private static IDbConnectionProvider InitDb(DirectoryInfo directory, QueueName queueName)
        {
            var dbpath = Path.Combine(directory.FullName, queueName + ".db");
            var connectionStringSettings = new ConnectionStringSettings
            {
                Name = dbpath,
                ConnectionString = "Data Source=" + dbpath + "; Version=3",
                ProviderName = "System.Data.SQLite"
            };
            return new SingletonConnectionProvider(connectionStringSettings);
        }

        protected override Task<SQLQueuedMessage> InsertQueuedMessage(Message message, IPrincipal senderPrincipal)
        {
            CheckDisposed();
            var op = new SQLiteOperation<SQLQueuedMessage>(() => base.InsertQueuedMessage(message, senderPrincipal));
            _operationQueue.Post(op);
            return op.Task;
        }

        protected override Task UpdateQueuedMessage(SQLQueuedMessage queuedMessage, DateTime? acknowledged, DateTime? abandoned, int attempts)
        {
            CheckDisposed();
            var op = new SQLiteOperation(() => base.UpdateQueuedMessage(queuedMessage, acknowledged, abandoned, attempts));
            _operationQueue.Post(op);
            return op.Task;
        }

        protected override Task<IEnumerable<SQLQueuedMessage>> SelectQueuedMessages()
        {
            CheckDisposed();
            var op = new SQLiteOperation<IEnumerable<SQLQueuedMessage>>(() => base.SelectQueuedMessages());
            _operationQueue.Post(op);
            return op.Task;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                    _operationQueue.Complete();
                }
                catch(Exception)
                {
                }
            }
            base.Dispose(disposing);
        }
    }
}
