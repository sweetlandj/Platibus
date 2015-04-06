using Platibus.Config.Extensibility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
