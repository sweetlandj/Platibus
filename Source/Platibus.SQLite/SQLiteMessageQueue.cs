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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Diagnostics;
using Platibus.Queueing;
using Platibus.Security;
using Platibus.SQL;
using Platibus.SQLite.Commands;
#if NET452
using System.Configuration;
#endif
#if NETSTANDARD2_0
using Platibus.Config;
#endif

namespace Platibus.SQLite
{
    /// <inheritdoc />
    /// <summary>
    /// A message queue based on a SQLite database
    /// </summary>
    public class SQLiteMessageQueue : SQLMessageQueue
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ICommandExecutor _commandExecutor = new SynchronizingCommandExecutor();

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.SQLite.SQLiteMessageQueue" />
        /// </summary>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="listener">The object that will process messages off of the queue</param>
        /// <param name="options">(Optional) Options for concurrency and retry limits</param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        ///     are reported and processed</param>
        /// <param name="baseDirectory">The directory in which the SQLite database will be created</param>
        /// <param name="securityTokenService">(Optional) A service for issuing security tokens
        ///     that can be stored with queued messages to preserve the security context in which
        ///     they were enqueued</param>
        /// <param name="messageEncryptionService"></param>
        public SQLiteMessageQueue(QueueName queueName,
            IQueueListener listener,
            QueueOptions options, IDiagnosticService diagnosticService, DirectoryInfo baseDirectory,
            ISecurityTokenService securityTokenService, IMessageEncryptionService messageEncryptionService)
            : base(queueName, listener, options, diagnosticService, InitConnectionProvider(baseDirectory, queueName, diagnosticService), 
                new SQLiteMessageQueueingCommandBuilders(), securityTokenService, messageEncryptionService)
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private static IDbConnectionProvider InitConnectionProvider(DirectoryInfo directory, QueueName queueName, IDiagnosticService diagnosticService)
        {
            var myDiagnosticService = diagnosticService ?? Diagnostics.DiagnosticService.DefaultInstance;
            var dbPath = Path.Combine(directory.FullName, queueName + ".db");
#if NET452
            var connectionStringSettings = new ConnectionStringSettings
            {
                Name = dbPath,
                ConnectionString = "Data Source=" + dbPath + "; Version=3; BinaryGUID=False; DateTimeKind=Utc",
                ProviderName = "System.Data.SQLite"
            };
#endif
#if NETSTANDARD2_0
            SQLiteProviderFactory.Register();
            var connectionStringSettings = new ConnectionStringSettings
            {
                Name = dbPath,
                ConnectionString = "Data Source=" + dbPath + "",
                ProviderName = SQLiteProviderFactory.InvariantName
            };
#endif

            return new SingletonConnectionProvider(connectionStringSettings, myDiagnosticService);
        }

        /// <inheritdoc />
        public override Task Init(CancellationToken cancellationToken = default(CancellationToken))
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
            return base.Init(cancellationToken);
        }

        /// <inheritdoc />
        protected override Task InsertQueuedMessage(QueuedMessage queuedMessage, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            return _commandExecutor.Execute(
                () => base.InsertQueuedMessage(queuedMessage, cancellationToken), 
                cancellationToken);
        }

        /// <inheritdoc />
        protected override Task DeleteQueuedMessage(QueuedMessage queuedMessage, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            return _commandExecutor.Execute(
                () => base.DeleteQueuedMessage(queuedMessage, cancellationToken), 
                cancellationToken);
        }

        /// <inheritdoc />
        protected override Task UpdateQueuedMessage(QueuedMessage queuedMessage, DateTime? abandoned, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            return _commandExecutor.Execute(
                () => base.UpdateQueuedMessage(queuedMessage, abandoned, cancellationToken),
                cancellationToken);
        }

        /// <inheritdoc />
        protected override Task<IEnumerable<QueuedMessage>> GetPendingMessages(CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            return _commandExecutor.ExecuteRead(
                () => base.GetPendingMessages(cancellationToken),
                cancellationToken);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            _cancellationTokenSource.Cancel();

            if (_commandExecutor is IDisposable disposableCommandExecutor)
            {
                disposableCommandExecutor.Dispose();
            }

            if (disposing)
            {
                _cancellationTokenSource.Dispose();
                
            }
            base.Dispose(disposing);
        }
    }
}