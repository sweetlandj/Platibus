using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Http;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// A <see cref="IDiagnosticService"/> implementation that records measurements in InfluxDB
    /// </summary>
    public class InfluxDBSink : IDiagnosticEventSink, IDisposable
    {
        private static readonly IDictionary<string, string> DefaultTags;

        static InfluxDBSink()
        {
            var assembly = Assembly.GetEntryAssembly() ??
                           Assembly.GetExecutingAssembly();

            DefaultTags = new Dictionary<string, string>
            {
                {"host", Environment.MachineName},
                {"app", assembly.GetName().Name},
                {"app_var", assembly.GetName().Version.ToString()}
            };
        }

        private readonly string _measurement;
        private readonly InfluxDBPrecision _precision;
        private readonly string _tagSet;
        private readonly Timer _measurementTimer;
        private readonly HttpClient _client;
       
        private Fields _fields = new Fields();
        
        private bool _disposed;

        /// <summary>
        /// Initializes a new <see cref="InfluxDBSink"/>
        /// </summary>
        /// <param name="options">Options specifying connection parameters and details governing
        /// how points are written to the InfluxDB database</param>
        /// <param name="sampleRate">(Optional) The rate at which samples should be taken</param>
        public InfluxDBSink(InfluxDBOptions options, TimeSpan sampleRate = default(TimeSpan))
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _measurement = string.IsNullOrWhiteSpace(options.Measurement) 
                ? InfluxDBOptions.DefaultMeasurement 
                : options.Measurement.Trim();

            _precision = options.Precision ?? InfluxDBPrecision.Nanosecond;

            var mySampleRate = sampleRate <= TimeSpan.Zero
                ? TimeSpan.FromSeconds(5)
                : sampleRate;

            var parameters = new Dictionary<string, string>();
            parameters["db"] = options.Database;
            parameters["u"] = options.Username;
            parameters["p"] = options.Password;
            parameters["precision"] = _precision.Name;

            var queryString = string.Join("&", parameters
                .Where(param => !string.IsNullOrWhiteSpace(param.Value))
                .Select(param => param.Key + "=" + param.Value));
            
            _client = new HttpClient
            {
                BaseAddress = new UriBuilder(options.Uri)
                {
                    Path = "write",
                    Query = queryString
                }.Uri
            };

            var myTags = options.Tags.Any() ? options.Tags : DefaultTags;
            var validTags = myTags.Where(tag => !string.IsNullOrWhiteSpace(tag.Key));
            _tagSet = string.Join(",", validTags.Select(tag => TagEscape(tag.Key) + "=" + TagEscape(tag.Value)));

            _measurementTimer = new Timer(_ => RecordMeasurements(), null, mySampleRate, mySampleRate);
        }

        private static string TagEscape(string tagKeyOrValue)
        {
            return string.IsNullOrWhiteSpace(tagKeyOrValue)
                ? tagKeyOrValue
                : tagKeyOrValue
                    .Replace(",", @"\,")
                    .Replace("=", @"\=")
                    .Replace(" ", @"\ ");
        }
        
        /// <inheritdoc />
        public void Consume(DiagnosticEvent @event)
        {
            IncrementCounters(@event);
        }

        /// <inheritdoc />
        public Task ConsumeAsync(DiagnosticEvent @event, CancellationToken cancellationToken = new CancellationToken())
        {
            IncrementCounters(@event);
            return Task.FromResult(0);
        }

        private void IncrementCounters(DiagnosticEvent @event)
        {
            switch (@event.Type.Level)
            {
                case DiagnosticEventLevel.Error:
                    Interlocked.Increment(ref _fields.Errors);
                    break;
                case DiagnosticEventLevel.Warn:
                    Interlocked.Increment(ref _fields.Warnings);
                    break;
            }

            if (@event.Type == HttpEventType.HttpRequestReceived)
            {
                Interlocked.Increment(ref _fields.Requests);
            }
            else if (@event.Type == DiagnosticEventType.MessageReceived)
            {
                Interlocked.Increment(ref _fields.Received);
            }
            else if (@event.Type == DiagnosticEventType.MessageAcknowledged)
            {
                Interlocked.Increment(ref _fields.Acknowledgements);
            }
            else if (@event.Type == DiagnosticEventType.MessageNotAcknowledged)
            {
                Interlocked.Increment(ref _fields.AcknowledgementFailures);
            }
            else if (@event.Type == DiagnosticEventType.MessageSent)
            {
                Interlocked.Increment(ref _fields.Sent);
            }
            else if (@event.Type == DiagnosticEventType.MessagePublished)
            {
                Interlocked.Increment(ref _fields.Published);
            }
            else if (@event.Type == DiagnosticEventType.MessageDelivered)
            {
                Interlocked.Increment(ref _fields.Delivered);
            }
            else if (@event.Type == DiagnosticEventType.MessageDeliveryFailed)
            {
                Interlocked.Increment(ref _fields.DeliveryFailures);
            }
            else if (@event.Type == DiagnosticEventType.MessageExpired)
            {
                Interlocked.Increment(ref _fields.Expired);
            }
            else if (@event.Type == DiagnosticEventType.DeadLetter)
            {
                Interlocked.Increment(ref _fields.Dead);
            }
        }

        /// <summary>
        /// Take measurements and record points in the InfluxDB database
        /// </summary>
        public void RecordMeasurements()
        {
            var timestamp = _precision.GetTimestamp();
            var fields = Interlocked.Exchange(ref _fields, new Fields());
            var fieldSet = "requests=" + fields.Requests
                           + ",received=" + fields.Received
                           + ",acknowledgements=" + fields.Acknowledgements
                           + ",acknowledgement_failures=" + fields.AcknowledgementFailures
                           + ",expired=" + fields.Expired
                           + ",dead=" + fields.Dead
                           + ",sent=" + fields.Sent
                           + ",delivered=" + fields.Delivered
                           + ",delivery_failures=" + fields.DeliveryFailures
                           + ",errors=" + fields.Errors
                           + ",warnings=" + fields.Warnings;

            var point = string.Join(",", _measurement, _tagSet) + " " + fieldSet + " " + timestamp;
            var response = _client.PostAsync("", new StringContent(point)).Result;
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Releases or frees managed and unmanaged resources
        /// </summary>
        /// <param name="disposing">Whether this method is called from <see cref="Dispose()"/></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _measurementTimer.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        ~InfluxDBSink()
        {
            Dispose(false);
        }
        
        private class Fields
        {
            public long Requests;
            public long Received;
            public long Acknowledgements;
            public long AcknowledgementFailures;
            public long Expired;
            public long Dead;
            public long Sent;
            public long Published;
            public long Delivered;
            public long DeliveryFailures;
            public long Errors;
            public long Warnings;
        }
    }
}