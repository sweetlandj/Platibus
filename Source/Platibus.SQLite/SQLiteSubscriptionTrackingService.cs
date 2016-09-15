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
    /// An <see cref="ISubscriptionTrackingService"/> implementation that reads and writes subscription
    /// information to a SQLite database
    /// </summary>
    public class SQLiteSubscriptionTrackingService : SQLSubscriptionTrackingService
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ActionBlock<ISQLiteOperation> _operationQueue;

        /// <summary>
        /// Initializes a new <see cref="SQLiteSubscriptionTrackingService"/>
        /// </summary>
        /// <param name="baseDirectory">The directory in which the SQLite database files will
        /// be created</param>
        /// <remarks>
        /// If a base directory is not specified then the base directory will default to a
        /// directory named <c>platibus\subscriptions</c> beneath the current app domain base 
        /// directory.  If the base directory does not exist it will be created.
        /// </remarks>
        public SQLiteSubscriptionTrackingService(DirectoryInfo baseDirectory)
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
        private static IDbConnectionProvider InitDb(DirectoryInfo baseDirectory)
        {
            if (baseDirectory == null)
            {
                var appdomainDirectory = AppDomain.CurrentDomain.BaseDirectory;
                baseDirectory = new DirectoryInfo(Path.Combine(appdomainDirectory, "platibus", "subscriptions"));
            }
            var dbPath = Path.Combine(baseDirectory.FullName, "subscriptions.db");
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
                    command.CommandText = new SQLiteDialect().CreateSubscriptionTrackingServiceObjectsCommand;
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                connectionProvider.ReleaseConnection(connection);
            }
            return connectionProvider;
        }

        /// <summary>
        /// Inserts or updates a subscription record in the SQL database
        /// </summary>
        /// <param name="topic">The topic to which the <paramref name="subscriber"/> is subscribing</param>
        /// <param name="subscriber">The base URI of the subscribing Platibus instance</param>
        /// <param name="expires">The date and time at which the subscription will expire</param>
        /// <returns>Returns a task that will complete when the subscription record has been inserted 
        /// or updated and whose result will be the an immutable representation of the inserted 
        /// subscription record</returns>
        protected override Task<SQLSubscription> InsertOrUpdateSubscription(TopicName topic, Uri subscriber,
            DateTime expires)
        {
            CheckDisposed();
            var op =
                new SQLiteOperation<SQLSubscription>(
                    () => base.InsertOrUpdateSubscription(topic, subscriber, expires));
            _operationQueue.Post(op);
            return op.Task;
        }

        /// <summary>
        /// Selects all of the non-expired subscription records from the SQL database
        /// </summary>
        /// <returns>Returns a task that will complete when the subscription records have been
        /// selected and whose result will be the records that were selected</returns>
        protected override Task<IEnumerable<SQLSubscription>> SelectSubscriptions()
        {
            CheckDisposed();
            var op = new SQLiteOperation<IEnumerable<SQLSubscription>>(() => base.SelectSubscriptions());
            _operationQueue.Post(op);
            return op.Task;
        }

        /// <summary>
        /// Deletes a subscription record from the SQL database
        /// </summary>
        /// <param name="topic">The name of the topic</param>
        /// <param name="subscriber">The base URI of the subscribing Platibus instance</param>
        /// <returns>Returns a task that will complete when the subscription record
        /// has been deleted</returns>
        protected override Task DeleteSubscription(TopicName topic, Uri subscriber)
        {
            CheckDisposed();
            var op = new SQLiteOperation(() => base.DeleteSubscription(topic, subscriber));
            _operationQueue.Post(op);
            return op.Task;
        }

        /// <summary>
        /// Called by the <see cref="SQLSubscriptionTrackingService.Dispose()"/> method 
        /// or by the finalizer to free held resources
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the 
        /// <see cref="SQLSubscriptionTrackingService.Dispose()"/> method (<c>true</c>) or
        /// from the finalizer (<c>false</c>)</param>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_cancellationTokenSource")]
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _operationQueue.Complete();
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.TryDispose();
        }
    }
}