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
using Platibus.SQL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Platibus.SQLite
{
    class SQLiteMessageQueue : SQLMessageQueue
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ActionBlock<ISQLiteOperation> _operationQueue;

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
            var dbPath = Path.Combine(directory.FullName, queueName + ".db");
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
                    command.CommandText = new SQLiteDialect().CreateMessageQueueingServiceObjectsCommand;
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                connectionProvider.ReleaseConnection(connection);
            }
            return connectionProvider;
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
                catch (Exception)
                {
                }
            }
            base.Dispose(disposing);
        }
    }
}
