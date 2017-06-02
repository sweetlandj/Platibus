// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
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
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Platibus.Security;
using Platibus.SQL;
using Platibus.SQLite.Commands;

namespace Platibus.SQLite
{
    /// <summary>
    /// A message queue based on a SQLite database
    /// </summary>
    public class SQLiteMessageQueue : SQLMessageQueue
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ActionBlock<ISQLiteOperation> _operationQueue;

        /// <summary>
        /// Initializes a new <see cref="SQLiteMessageQueue"/>
        /// </summary>
        /// <param name="baseDirectory">The directory in which the SQLite database will be created</param>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="listener">The object that will process messages off of the queue</param>
        /// <param name="securityTokenService">(Optional) A service for issuing security tokens
        /// that can be stored with queued messages to preserve the security context in which
        /// they were enqueued</param>
        /// <param name="options">(Optional) Options for concurrency and retry limits</param>
        public SQLiteMessageQueue(DirectoryInfo baseDirectory, QueueName queueName, IQueueListener listener, ISecurityTokenService securityTokenService, QueueOptions options = null)
            : base(InitConnectionProvider(baseDirectory, queueName), new SQLiteMessageQueueingCommandBuilders(), queueName, listener, securityTokenService, options)
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

        private static IDbConnectionProvider InitConnectionProvider(DirectoryInfo directory, QueueName queueName)
        {
            var dbPath = Path.Combine(directory.FullName, queueName + ".db");
            var connectionStringSettings = new ConnectionStringSettings
            {
                Name = dbPath,
                ConnectionString = "Data Source=" + dbPath + "; Version=3; BinaryGUID=False; DateTimeKind=Utc",
                ProviderName = "System.Data.SQLite"
            };

            return new SingletonConnectionProvider(connectionStringSettings);
        }

        /// <inheritdoc />
        public override Task Init()
        {
            // A separate database file is created for each queue, so the object initialization
            // commands must be done once for each queue.

            var conection = ConnectionProvider.GetConnection();
            try
            {
                var commandBuilder = CommandBuilders.NewCreateObjectsCommandBuilder();
                using (var command = commandBuilder.BuildDbCommand(conection))
                {
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                ConnectionProvider.ReleaseConnection(conection);
            }
            return base.Init();
        }

        /// <inheritdoc />
        protected override Task<SQLQueuedMessage> InsertQueuedMessage(Message message, IPrincipal principal)
        {
            CheckDisposed();
            var op = new SQLiteOperation<SQLQueuedMessage>(() => base.InsertQueuedMessage(message, principal));
            _operationQueue.Post(op);
            return op.Task;
        }

        /// <inheritdoc />
        protected override Task UpdateQueuedMessage(SQLQueuedMessage queuedMessage, DateTime? acknowledged,
            DateTime? abandoned, int attempts)
        {
            CheckDisposed();
            var op =
                new SQLiteOperation(() => base.UpdateQueuedMessage(queuedMessage, acknowledged, abandoned, attempts));
            _operationQueue.Post(op);
            return op.Task;
        }

        /// <inheritdoc />
        protected override Task<IEnumerable<SQLQueuedMessage>> SelectQueuedMessages()
        {
            CheckDisposed();
            var op = new SQLiteOperation<IEnumerable<SQLQueuedMessage>>(() => base.SelectQueuedMessages());
            _operationQueue.Post(op);
            return op.Task;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_cancellationTokenSource")]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.TryDispose();
                _operationQueue.Complete();
            }
            base.Dispose(disposing);
        }
    }
}