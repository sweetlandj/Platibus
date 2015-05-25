using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using Common.Logging;

namespace Platibus.SQL
{
    public class DefaultConnectionProvider : IDbConnectionProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.SQL);
        private readonly ConnectionStringSettings _connectionStringSettings;
        private readonly DbProviderFactory _providerFactory;

        private bool _disposed;

        protected ConnectionStringSettings ConnectionStringSettings
        {
            get { return _connectionStringSettings; }
        }

        protected DbProviderFactory ProviderFactory
        {
            get { return _providerFactory; }
        }

        public DefaultConnectionProvider(ConnectionStringSettings connectionStringSettings)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException("connectionStringSettings");
            _connectionStringSettings = connectionStringSettings;
            _providerFactory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);
        }

        public virtual DbConnection GetConnection()
        {
            CheckDisposed();
            var connection = _providerFactory.CreateConnection();
            if (connection != null)
            {
                connection.ConnectionString = _connectionStringSettings.ConnectionString;
                connection.Open();
            }
            return connection;
        }

        public virtual void ReleaseConnection(DbConnection connection)
        {
            CheckDisposed();
            if (connection != null && connection.State == ConnectionState.Open)
            {
                try
                {
                    connection.Close();
                }
                catch (Exception ex)
                {
                    Log.Warn("Error closing database connection", ex);
                }
            }
        }

        protected void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        ~DefaultConnectionProvider()
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
        }
    }
}