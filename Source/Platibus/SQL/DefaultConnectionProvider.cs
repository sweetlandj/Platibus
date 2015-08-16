using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using Common.Logging;

namespace Platibus.SQL
{
    /// <summary>
    /// A connection provider that creates a new connection via the ADO.NET provider factory
    /// and closes the connection when released
    /// </summary>
    public class DefaultConnectionProvider : IDbConnectionProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.SQL);
        private readonly ConnectionStringSettings _connectionStringSettings;
        private readonly DbProviderFactory _providerFactory;

        /// <summary>
        /// The connection string settings for this connection provider
        /// </summary>
        protected ConnectionStringSettings ConnectionStringSettings
        {
            get { return _connectionStringSettings; }
        }

        /// <summary>
        /// The ADO.NET provider factory
        /// </summary>
        protected DbProviderFactory ProviderFactory
        {
            get { return _providerFactory; }
        }

        /// <summary>
        /// Initializes a new <see cref="DefaultConnectionProvider"/> with the specified
        /// <paramref name="connectionStringSettings"/>
        /// </summary>
        /// <param name="connectionStringSettings">The connection string settings for which
        /// the connection provider will provide connections</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connectionStringSettings"/>
        /// is <c>null</c></exception>
        public DefaultConnectionProvider(ConnectionStringSettings connectionStringSettings)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException("connectionStringSettings");
            _connectionStringSettings = connectionStringSettings;
            _providerFactory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);
        }

        /// <summary>
        /// Produces a database connection
        /// </summary>
        /// <returns>A database connection</returns>
        /// <remarks>
        /// The return value of this method may be <c>null</c> depending on whether the ADO.NET
        /// provider has overridden the virtual <c>DbProviderFactory.CreateConnection</c>
        /// method.
        /// </remarks>
        public virtual DbConnection GetConnection()
        {
            var connection = _providerFactory.CreateConnection();

            // The abstract DbProviderFactory class specifies a virtual CreateConnection method
            // whose default implementation returns null.  There is a possibility that this method
            // will not be overridden by the concrete provider factory.  If that is the case, then 
            // there is nothing we can do other than avoid a NullReferenceException by not attempting 
            // to set the connection string or open the connection.
            if (connection != null)
            {
                connection.ConnectionString = _connectionStringSettings.ConnectionString;
                connection.Open();
            }
            return connection;
        }

        /// <summary>
        /// Releases a database connection
        /// </summary>
        /// <param name="connection">The connection to release</param>
        public virtual void ReleaseConnection(DbConnection connection)
        {
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
    }
}