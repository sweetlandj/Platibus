using System;
using System.Configuration;

namespace Platibus.SQL
{
    /// <summary>
    /// An interface describing a class that provides a SQL dialect that is appropriate
    /// for a given set of connection string settings
    /// </summary>
    public interface ISQLDialectProvider
    {
        /// <summary>
        /// Returns the dialect most appropriate given the specified 
        /// <paramref name="connectionStringSettings"/>
        /// </summary>
        /// <param name="connectionStringSettings">The connection string settings</param>
        /// <returns>A SQL dialect appropriate for use with connections based on the
        /// specified <paramref name="connectionStringSettings"/></returns>
        /// <exception cref="ArgumentNullException">Thrown if 
        /// <paramref name="connectionStringSettings"/> is <c>null</c></exception>
        ISQLDialect GetSQLDialect(ConnectionStringSettings connectionStringSettings);
    }
}