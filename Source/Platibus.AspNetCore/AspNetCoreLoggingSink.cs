using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Platibus.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.AspNetCore
{
    /// <inheritdoc />
    /// <summary>
    /// An <see cref="T:Platibus.Diagnostics.IDiagnosticEventSink" /> implementation that 
    /// outputs to an ASP.NET Core <see cref="T:Microsoft.Extensions.Logging.ILogger" />
    /// </summary>
    public class AspNetCoreLoggingSink : IDiagnosticEventSink
    {
        private readonly ILogger _logger;

        public AspNetCoreLoggingSink(ILogger<AspNetCoreLoggingSink> logger)
        {
            _logger = logger;
        }

        public void Consume(DiagnosticEvent @event)
        {
            var logLevel = GetLogLevel(@event);
            var eventId = GetEventId(@event);
            var state = GetState(@event);
            _logger.Log(logLevel, eventId, state, @event.Exception, Format);
        }

        public Task ConsumeAsync(DiagnosticEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            Consume(@event);
            return Task.CompletedTask;
        }

        private static LogLevel GetLogLevel(DiagnosticEvent e)
        {
            switch (e.Type.Level)
            {
                case DiagnosticEventLevel.Trace:
                    return LogLevel.Trace;
                case DiagnosticEventLevel.Debug:
                    return LogLevel.Debug;
                case DiagnosticEventLevel.Info:
                    return LogLevel.Information;
                case DiagnosticEventLevel.Warn:
                    return LogLevel.Warning;
                case DiagnosticEventLevel.Error:
                    return LogLevel.Error;
                default:
                    return LogLevel.Information;
            }
        }

        private static EventId GetEventId(DiagnosticEvent e)
        {
            return new EventId(e.Type.GetHashCode(), e.Type.Name);
        }

        private static object GetState(DiagnosticEvent e)
        {
            return new
            {
                e.Message?.Headers.MessageId,
                e.Message?.Headers.MessageName,
                e.Message?.Headers.Origination,
                e.Message?.Headers.Destination,
                e.Message?.Headers.ContentType,
                e.Exception,
                e.Queue,
                e.Topic
            };
        }

        private static string Format(object state, Exception e)
        {
            var data = new
            {
                State = state,
                Exception = e
            };
            return JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}
