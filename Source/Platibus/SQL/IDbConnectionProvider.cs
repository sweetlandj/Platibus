using System;
using System.Data.Common;

namespace Platibus.SQL
{
    public interface IDbConnectionProvider : IDisposable
    {
        DbConnection GetConnection();
        void ReleaseConnection(DbConnection connection);
    }
}