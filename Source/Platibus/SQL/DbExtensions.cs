using Common.Logging;
using Platibus.Config.Extensibility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.SQL
{
    public static class DbExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.SQL);

        public static DbConnection OpenConnection(this ConnectionStringSettings connectionStringSettings)
        {
            var providerFactory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);
            var connection = providerFactory.CreateConnection();
            if (connection != null)
            {
                connection.ConnectionString = connectionStringSettings.ConnectionString;
                connection.Open();
            }
            return connection;
        }

        public static void SetParameter(this DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;

            if (value == null)
            {
                parameter.Value = DBNull.Value;
            }
            else if (value is TimeSpan)
            {
                parameter.Value = (long)((TimeSpan)value).TotalMilliseconds;
            }
            else
            { 
                parameter.Value = value; 
            }

            command.Parameters.Add(parameter);
        }

        public static string GetString(this IDataRecord record, string name, string defaultValue = null)
        {
            var ordinal = record.GetOrdinal(name);
            return record.IsDBNull(ordinal) ? defaultValue : record.GetString(name);
        }

        public static int GetInt(this IDataRecord record, string name, int defaultValue = default(int))
        {
            var ordinal = record.GetOrdinal(name);
            return record.IsDBNull(ordinal) ? defaultValue : record.GetInt32(ordinal);
        }

        public static long GetLong(this IDataRecord record, string name, long defaultValue = default(long))
        {
            var ordinal = record.GetOrdinal(name);
            return record.IsDBNull(ordinal) ? defaultValue : record.GetInt64(ordinal);
        }

        public static bool GetBoolean(this IDataRecord record, string name, bool defaultValue = default(bool))
        {
            var ordinal = record.GetOrdinal(name);
            return record.IsDBNull(ordinal) ? defaultValue : record.GetBoolean(name);
        }

        public static DateTime GetDateTime(this IDataRecord record, string name, DateTime defaultValue = default(DateTime))
        {
            var ordinal = record.GetOrdinal(name);
            return record.IsDBNull(ordinal) ? defaultValue : record.GetDateTime(name);
        }

        public static TimeSpan GetTimeSpan(this IDataRecord record, string name, TimeSpan defaultValue = default(TimeSpan))
        {
            var milliseconds = record.GetLong(name);
            return TimeSpan.FromMilliseconds(milliseconds);
        }

        public static ISQLDialect GetSQLDialect(this ConnectionStringSettings connectionStringSettings)
        {
            var providerName = connectionStringSettings.ProviderName;
            ISQLDialectProvider provider;
            if (string.IsNullOrWhiteSpace(providerName))
            {
                Log.Debug("No connection string provider specified; using default provider...");
                provider = new MSSQLDialectProvider();
            }
            else
            {
                provider = ProviderHelper.GetProvider<ISQLDialectProvider>(providerName);
            }

            Log.Debug("Getting SQL dialect...");
            return provider.GetSQLDialect(connectionStringSettings);
        }
    }
}
