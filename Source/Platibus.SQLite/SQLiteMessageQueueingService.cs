using Common.Logging;
using Platibus.SQL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Platibus.SQLite
{
    public class SQLiteMessageQueueingService : IMessageQueueingService
    {
        private static readonly ILog Log = LogManager.GetLogger(SQLiteLoggingCategories.SQLite);

        private readonly DirectoryInfo _baseDirectory;
        private readonly ConcurrentDictionary<QueueName, SQLiteMessageQueue> _queues = new ConcurrentDictionary<QueueName, SQLiteMessageQueue>();

        private bool _disposed;

        public SQLiteMessageQueueingService(DirectoryInfo baseDirectory)
        {
            if (baseDirectory == null)
            {
                var appdomainDirectory = AppDomain.CurrentDomain.BaseDirectory;
                baseDirectory = new DirectoryInfo(Path.Combine(appdomainDirectory, "platibus", "queues"));
            }
            _baseDirectory = baseDirectory;
        }

        public void Init()
        {
            _baseDirectory.Refresh();
            if (_baseDirectory.Exists)
            {
                _baseDirectory.Create();
                _baseDirectory.Refresh();
            }
        }

        public async Task CreateQueue(QueueName queueName, IQueueListener listener, QueueOptions options = default(QueueOptions))
        {
            CheckDisposed();
            var queue = new SQLiteMessageQueue(_baseDirectory, queueName, listener, options);
            if (!_queues.TryAdd(queueName, queue))
            {
                throw new QueueAlreadyExistsException(queueName);
            }

            await queue.Init().ConfigureAwait(false);
            Log.DebugFormat("SQLite queue \"{0}\" created successfully", queueName);
        }

        public async Task EnqueueMessage(QueueName queueName, Message message, IPrincipal senderPrincipal)
        {
            SQLiteMessageQueue queue;
            if (!_queues.TryGetValue(queueName, out queue)) throw new QueueNotFoundException(queueName);

            Log.DebugFormat("Enqueueing message ID {0} in SQL queue \"{1}\"...", message.Headers.MessageId, queueName);
            await queue.Enqueue(message, senderPrincipal).ConfigureAwait(false);
            Log.DebugFormat("Message ID {0} enqueued successfully in SQL queue \"{1}\"", message.Headers.MessageId, queueName);
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
            if (disposing)
            {
                foreach(var queue in _queues.Values)
                {
                    queue.Dispose();
                }
            }
        }

        protected void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
