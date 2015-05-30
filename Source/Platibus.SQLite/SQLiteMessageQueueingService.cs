// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace Platibus.SQLite
{
    public class SQLiteMessageQueueingService : IMessageQueueingService
    {
        private static readonly ILog Log = LogManager.GetLogger(SQLiteLoggingCategories.SQLite);

        private readonly DirectoryInfo _baseDirectory;

        private readonly ConcurrentDictionary<QueueName, SQLiteMessageQueue> _queues =
            new ConcurrentDictionary<QueueName, SQLiteMessageQueue>();

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
            if (!_baseDirectory.Exists)
            {
                _baseDirectory.Create();
                _baseDirectory.Refresh();
            }
        }

        public async Task CreateQueue(QueueName queueName, IQueueListener listener, QueueOptions options = default(QueueOptions), CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            var queue = new SQLiteMessageQueue(_baseDirectory, queueName, listener, options);
            if (!_queues.TryAdd(queueName, queue))
            {
                throw new QueueAlreadyExistsException(queueName);
            }

            await queue.Init();
            Log.DebugFormat("SQLite queue \"{0}\" created successfully", queueName);
        }

        public async Task EnqueueMessage(QueueName queueName, Message message, IPrincipal senderPrincipal, CancellationToken cancellationToken = default(CancellationToken))
        {
            SQLiteMessageQueue queue;
            if (!_queues.TryGetValue(queueName, out queue)) throw new QueueNotFoundException(queueName);

            Log.DebugFormat("Enqueueing message ID {0} in SQL queue \"{1}\"...", message.Headers.MessageId, queueName);
            await queue.Enqueue(message, senderPrincipal);
            Log.DebugFormat("Message ID {0} enqueued successfully in SQL queue \"{1}\"", message.Headers.MessageId,
                queueName);
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
                foreach (var queue in _queues.Values)
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