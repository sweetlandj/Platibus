using System;
using System.Data.Common;

namespace Platibus.SQL
{
    /// <summary>
    /// An interface that describes a database connection factory
    /// </summary>
    public interface IDbConnectionProvider
    {
        /// <summary>
        /// Produces a database connection
        /// </summary>
        /// <returns>A database connection</returns>
        DbConnection GetConnection();

        /// <summary>
        /// Releases a database connection
        /// </summary>
        /// <param name="connection">The connection to release</param>
        void ReleaseConnection(DbConnection connection);
    }
}