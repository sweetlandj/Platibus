using System.Configuration;

namespace Platibus.SQL
{
    public interface ISQLDialectProvider
    {
        ISQLDialect GetSQLDialect(ConnectionStringSettings connectionStringSettings);
    }
}