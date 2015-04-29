using System.Configuration;
using Platibus.Config.Extensibility;

namespace Platibus.SQL
{
    [Provider("System.Data.SQLClient")]
    public class MSSQLDialectProvider : ISQLDialectProvider
    {

        public ISQLDialect GetSQLDialect(ConnectionStringSettings connectionStringSettings)
        {
            return new MSSQLDialect();
        }
    }
}
