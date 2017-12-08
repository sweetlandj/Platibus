#if NETSTANDARD2_0
using Platibus.SQL;

namespace Platibus.SQLite
{
    /// <summary>
    /// Helper class for ensuring that the SQLite provider factory
    /// is registered
    /// </summary>
    internal static class SQLiteProviderFactory
    {
        public const string InvariantName = "System.Data.SQLite";

        internal static void Register()
        {
            // Register the provider factory to resolution based on provider 
            // name in applications that target .NET Standard 2.0
            DbProviderFactories.Add(InvariantName, () => Microsoft.Data.Sqlite.SqliteFactory.Instance);
        }
    }
}
#endif