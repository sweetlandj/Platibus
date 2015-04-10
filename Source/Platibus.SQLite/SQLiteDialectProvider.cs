using Platibus.Config.Extensibility;
using Platibus.SQL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.SQLite
{
    [Provider("System.Data.SQLite")]
    public class SQLiteDialectProvider : ISQLDialectProvider
    {
        public ISQLDialect GetSQLDialect(ConnectionStringSettings connectionStringSettings)
        {
            return new SQLiteDialect();
        }
    }
}
