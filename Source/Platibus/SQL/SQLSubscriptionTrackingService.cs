
using Common.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Platibus.SQL
{
    public class SQLSubscriptionTrackingService : ISubscriptionTrackingService, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.SQL);

        private readonly IDbConnectionProvider _connectionProvider;
        private readonly ISQLDialect _dialect;
        private readonly ConcurrentDictionary<TopicName, IEnumerable<SQLSubscription>> _subscriptions = new ConcurrentDictionary<TopicName, IEnumerable<SQLSubscription>>();

        private bool _disposed;

        public IDbConnectionProvider ConnectionProvider
        {
            get { return _connectionProvider; }
        }

        public ISQLDialect Dialect
        {
            get { return _dialect; }
        }

        public SQLSubscriptionTrackingService(ConnectionStringSettings connectionStringSettings, ISQLDialect dialect = null)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException("connectionStringSettings");
            _connectionProvider = new DefaultConnectionProvider(connectionStringSettings);
            _dialect = dialect ?? connectionStringSettings.GetSQLDialect();
        }

        public SQLSubscriptionTrackingService(IDbConnectionProvider connectionProvider, ISQLDialect dialect)
        {
            if (connectionProvider == null) throw new ArgumentNullException("connectionProvider");
            if (dialect == null) throw new ArgumentNullException("dialect");
            _connectionProvider = connectionProvider;
            _dialect = dialect;
        }

        public async Task Init()
        {
            var connection = _connectionProvider.GetConnection();
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = _dialect.CreateSubscriptionTrackingServiceObjectsCommand;
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                _connectionProvider.ReleaseConnection(connection);
            }

            var subscriptionsByTopicName = (await SelectSubscriptions().ConfigureAwait(false))
                .GroupBy(subscription => subscription.TopicName);

            foreach (var grouping in subscriptionsByTopicName)
            {
                var topicName = grouping.Key;
                var subscriptions = grouping.ToList();
                _subscriptions.AddOrUpdate(topicName, subscriptions, (t, s) => s.Union(subscriptions).ToList());
            }
        }

        public async Task AddSubscription(TopicName topic, Uri subscriber, TimeSpan ttl = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            var expires = ttl <= TimeSpan.Zero ? DateTime.MaxValue : (DateTime.UtcNow + ttl);
            var subscription = await InsertOrUpdateSubscription(topic, subscriber, expires).ConfigureAwait(false);
            _subscriptions.AddOrUpdate(topic, new[] { subscription },
                (t, existing) => new[] { subscription }.Union(existing).ToList());
        }

        public async Task RemoveSubscription(TopicName topic, Uri subscriber, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            await DeleteSubscription(topic, subscriber);

            _subscriptions.AddOrUpdate(topic, new SQLSubscription[0],
                (t, existing) => existing.Where(se => se.Subscriber != subscriber).ToList());
        }

        public Task<IEnumerable<Uri>> GetSubscribers(TopicName topic, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            IEnumerable<SQLSubscription> subscriptions;
            _subscriptions.TryGetValue(topic, out subscriptions);
            var activeSubscribers = (subscriptions ?? Enumerable.Empty<SQLSubscription>())
                .Where(s => s.Expires > DateTime.UtcNow)
                .Select(s => s.Subscriber);

            return Task.FromResult(activeSubscribers);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        protected virtual Task<SQLSubscription> InsertOrUpdateSubscription(TopicName topicName, Uri subscriber, DateTime expires)
        {
            SQLSubscription subscription = null;
            var connection = _connectionProvider.GetConnection();
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = _dialect.InsertSubscriptionCommand;

                        command.SetParameter(_dialect.TopicNameParameterName, (string)topicName);
                        command.SetParameter(_dialect.SubscriberParameterName, subscriber.ToString());
                        command.SetParameter(_dialect.ExpiresParameterName, expires);

                        var rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            command.CommandText = _dialect.UpdateSubscriptionCommand;
                            command.ExecuteNonQuery();
                        }
                    }
                    subscription = new SQLSubscription(topicName, subscriber, expires);
                    scope.Complete();
                }
            }
            finally
            {
                _connectionProvider.ReleaseConnection(connection);
            }
            return Task.FromResult(subscription);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        protected virtual Task<IEnumerable<SQLSubscription>> SelectSubscriptions()
        {
            var subscriptions = new List<SQLSubscription>();
            var connection = _connectionProvider.GetConnection();
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = _dialect.SelectSubscriptionsCommand;
                        command.SetParameter(_dialect.CurrentDateParameterName, DateTime.UtcNow);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var topicName = (TopicName)reader.GetString("TopicName");
                                var subscriber = new Uri(reader.GetString("Subscriber"));
                                var expires = reader.GetDateTime("Expires").GetValueOrDefault(DateTime.MaxValue);
                                var subscription = new SQLSubscription(topicName, subscriber, expires);
                                subscriptions.Add(subscription);
                            }
                        }
                    }
                    scope.Complete();
                }
            }
            finally
            {
                _connectionProvider.ReleaseConnection(connection);
            }
            return Task.FromResult<IEnumerable<SQLSubscription>>(subscriptions);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        protected virtual Task DeleteSubscription(TopicName topicName, Uri subscriber)
        {
            var deleted = false;
            var connection = _connectionProvider.GetConnection();
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = _dialect.DeleteSubscriptionCommand;

                        command.SetParameter(_dialect.TopicNameParameterName, (string)topicName);
                        command.SetParameter(_dialect.SubscriberParameterName, subscriber.ToString());

                        var rowsAffected = command.ExecuteNonQuery();
                        deleted = rowsAffected > 0;
                    }
                    scope.Complete();
                }
            }
            finally
            {
                _connectionProvider.ReleaseConnection(connection);
            }
            return Task.FromResult(deleted);
        }

        ~SQLSubscriptionTrackingService()
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
                _connectionProvider.Dispose();
            }
        }

        protected void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
