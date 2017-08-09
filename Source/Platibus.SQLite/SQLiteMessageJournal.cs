// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
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
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Platibus.Diagnostics;
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
        /// <param name="baseDirectory">The directory in which the SQLite database files will be 
        ///     created</param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        ///     are reported and processed</param>
        /// <remarks>
        /// If a base directory is not specified then the base directory will default to a
        /// directory named <c>platibus\queues</c> beneath the current app domain base 
        /// directory.  If the base directory does not exist it will be created.
        /// </remarks>
        public SQLiteMessageJournal(DirectoryInfo baseDirectory, IDiagnosticService diagnosticService = null)
            : base(InitConnectionProvider(baseDirectory, diagnosticService), new SQLiteMessageJournalCommandBuilders(), diagnosticService)
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
        
        private static IDbConnectionProvider InitConnectionProvider(DirectoryInfo directory, IDiagnosticService diagnosticService)
        {
            if (directory == null)
            {
                var appDomainDirectory = AppDomain.CurrentDomain.BaseDirectory;
                directory = new DirectoryInfo(Path.Combine(appDomainDirectory, "platibus", "journal"));
            }

            directory.Refresh();
            if (!directory.Exists)
            {
                directory.Create();
            }

            var dbPath = Path.Combine(directory.FullName, "journal.db");
            var connectionStringSettings = new ConnectionStringSettings
            {
                Name = dbPath,
                ConnectionString = "Data Source=" + dbPath + "; Version=3; BinaryGUID=False; DateTimeKind=Utc",
                ProviderName = "System.Data.SQLite"
            };

            return new SingletonConnectionProvider(connectionStringSettings, diagnosticService);
        }

        /// <inheritdoc />
        public override async Task Append(Message message, MessageJournalCategory category,
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
        protected virtual void Dispose(bool disposing)
        {
            _cancellationTokenSource.Cancel();
            if (disposing)
            {
                _cancellationTokenSource.Dispose();
            }
        }
    }
}