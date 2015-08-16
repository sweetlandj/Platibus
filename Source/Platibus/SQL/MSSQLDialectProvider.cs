using System;
using System.Configuration;
using Platibus.Config.Extensibility;

namespace Platibus.SQL
{
    /// <summary>
    /// A <see cref="ISQLDialectProvider"/> for the System.Data.SQLClient ADO.NET provider
    /// </summary>
    [Provider("System.Data.SQLClient")]
    public class MSSQLDialectProvider : ISQLDialectProvider
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
        public ISQLDialect GetSQLDialect(ConnectionStringSettings connectionStringSettings)
        {
            return new MSSQLDialect();
        }
    }
}