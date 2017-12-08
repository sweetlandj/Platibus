using System;
using System.Collections.Generic;
using System.Linq;
#if NET452
using System.Configuration;
#endif
using System.Threading.Tasks;
#if NETSTANDARD2_0
using Microsoft.Extensions.Configuration;
#endif
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
#if NET452
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
#else
        /// <inheritdoc/>
        public Task<IDiagnosticEventSink> CreateDiagnosticEventSink(IConfiguration configuration)
        {
            var uri = configuration?.GetValue<Uri>("uri");
            var database = configuration?["database"];
            if (uri == null) throw new ConfigurationErrorsException("'uri' attribute is required (e.g. http://localhost:8086)");
            if (string.IsNullOrWhiteSpace(database)) throw new ConfigurationErrorsException("'database' attribute is required");

            var options = new InfluxDBOptions(uri, database)
            {
                Measurement = configuration["measurement"],
                Username = configuration["username"],
                Password = configuration["password"]
            };

            var precision = configuration["precision"];
            if (!string.IsNullOrWhiteSpace(precision))
            {
                options.Precision = InfluxDBPrecision.Parse(precision);
            }

            var tags = configuration["tags"];
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
            var sampleRateStr = configuration["sampleRate"];
            if (!string.IsNullOrWhiteSpace(sampleRateStr))
            {
                if (!TimeSpan.TryParse(sampleRateStr, out sampleRate))
                {
                    throw new ConfigurationErrorsException("Invalid timespan specified for 'sampleRate'");
                }
            }

            return Task.FromResult<IDiagnosticEventSink>(new InfluxDBSink(options, sampleRate));
        }
#endif
    }
}
