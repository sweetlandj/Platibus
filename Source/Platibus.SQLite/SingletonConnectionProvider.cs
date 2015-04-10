using Common.Logging;
using Platibus.SQL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.SQLite
{
    class SingletonConnectionProvider : IDbConnectionProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(SQLiteLoggingCategories.SQLite);
        private readonly DbConnection _connection;

        private bool _disposed;

        public SingletonConnectionProvider(DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            _connection = connection;
        }

        public SingletonConnectionProvider(ConnectionStringSettings connectionStringSettings)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException("connectionStringSettings");
            _connection = connectionStringSettings.OpenConnection();
        }

        public DbConnection GetConnection()
        {
            CheckDisposed();
            return _connection;
        }

        public void ReleaseConnection(DbConnection connection)
        {
        }

        protected void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        ~SingletonConnectionProvider()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            GC.SuppressFinalize(true);
            _disposed = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                _connection.Close();
            }
            catch(Exception ex)
            {
                Log.Warn("Error closing singleton connection", ex);
            }
        }
    }
}
