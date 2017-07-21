using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Platibus.Config;
using Platibus.Config.Extensibility;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// Provider for <see cref="IDiagnosticEventSink"/> implementations targeting InfluxDB
    /// </summary>
    [Provider("InfluxDB")]
    public class InfluxDBSinkProvider : IDiagnosticEventSinkProvider
    {
        /// <inheritdoc/>
        public Task<IDiagnosticEventSink> CreateDiagnosticEventSink(DiagnosticEventSinkElement configuration)
        {
            var uri = configuration.GetUri("uri");
            var database = configuration.GetString("database");
            if (uri == null) throw new ConfigurationErrorsException("'uri' attribute is required (e.g. http://localhost:8086)");
            if (string.IsNullOrWhiteSpace(database)) throw new ConfigurationErrorsException("'database' attribute is required");

            var options = new InfluxDBOptions(uri, database)
            {
                Measurement = configuration.GetString("measurement"),
                Username = configuration.GetString("username"),
                Password = configuration.GetString("password")
            };

            var precision = configuration.GetString("precision");
            if (!string.IsNullOrWhiteSpace(precision))
            {
                options.Precision = InfluxDBPrecision.Parse(precision);
            }
            
            var tags = configuration.GetString("tags");
            if (!string.IsNullOrWhiteSpace(tags))
            {
                options.Tags = tags.Split(',')
                    .Select(tag =>
                    {
                        var keyValue = tag.Split('=');
                        var key = keyValue.FirstOrDefault();
                        var value = keyValue.Skip(1).FirstOrDefault();
                        return new KeyValuePair<string, string>(key, value);
                    })
                    .ToDictionary(tag => tag.Key, tag => tag.Value);
            }

            var sampleRate = default(TimeSpan);
            var sampleRateStr = configuration.GetString("sampleRate");
            if (!string.IsNullOrWhiteSpace(sampleRateStr))
            {
                if (!TimeSpan.TryParse(sampleRateStr, out sampleRate))
                {
                    throw new ConfigurationErrorsException("Invalid timespan specified for 'sampleRate'");
                }
            }
            
            return Task.FromResult<IDiagnosticEventSink>(new InfluxDBSink(options, sampleRate));
        }
    }
}
