using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.SQL
{
    public interface IDbConnectionProvider : IDisposable
    {
        DbConnection GetConnection();
        void ReleaseConnection(DbConnection connection);
    }
}
