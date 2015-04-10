using Platibus.SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.SQLite
{
    class SQLiteDialect : CommonSQLDialect
    {
        public override string CreateObjectsCommand
        {
            get { return SQLiteCommands.CreateObjectsCommand; }
        }
    }
}
