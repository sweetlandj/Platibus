using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using Common.Logging;
using Platibus.Config.Extensibility;

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
                parameter.Value = (long) ((TimeSpan) value).TotalMilliseconds;
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
            return record.IsDBNull(ordinal) ? defaultValue : record.GetString(ordinal);
        }

        public static int? GetInt(this IDataRecord record, string name)
        {
            var ordinal = record.GetOrdinal(name);
            return record.IsDBNull(ordinal) ? null : (int?) record.GetInt32(ordinal);
        }

        public static long? GetLong(this IDataRecord record, string name)
        {
            var ordinal = record.GetOrdinal(name);
            return record.IsDBNull(ordinal) ? null : (long?) record.GetInt64(ordinal);
        }

        public static bool? GetBoolean(this IDataRecord record, string name)
        {
            var ordinal = record.GetOrdinal(name);
            return record.IsDBNull(ordinal) ? null : (bool?) record.GetBoolean(ordinal);
        }

        public static DateTime? GetDateTime(this IDataRecord record, string name)
        {
            var ordinal = record.GetOrdinal(name);
            return record.IsDBNull(ordinal) ? null : (DateTime?) record.GetDateTime(ordinal);
        }

        public static TimeSpan? GetTimeSpan(this IDataRecord record, string name)
        {
            var milliseconds = record.GetLong(name);
            return milliseconds == null ? null : (TimeSpan?) TimeSpan.FromMilliseconds(milliseconds.Value);
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