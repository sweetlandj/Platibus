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
    public class SQLiteSubscriptionTrackingService : SQLSubscriptionTrackingService
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ActionBlock<ISQLiteOperation> _operationQueue;

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

        protected override Task<SQLSubscription> InsertOrUpdateSubscription(TopicName topicName, Uri subscriber,
            DateTime expires)
        {
            CheckDisposed();
            var op =
                new SQLiteOperation<SQLSubscription>(
                    () => base.InsertOrUpdateSubscription(topicName, subscriber, expires));
            _operationQueue.Post(op);
            return op.Task;
        }

        protected override Task<IEnumerable<SQLSubscription>> SelectSubscriptions()
        {
            CheckDisposed();
            var op = new SQLiteOperation<IEnumerable<SQLSubscription>>(() => base.SelectSubscriptions());
            _operationQueue.Post(op);
            return op.Task;
        }

        protected override Task DeleteSubscription(TopicName topicName, Uri subscriber)
        {
            CheckDisposed();
            var op = new SQLiteOperation(() => base.DeleteSubscription(topicName, subscriber));
            _operationQueue.Post(op);
            return op.Task;
        }
    }
}