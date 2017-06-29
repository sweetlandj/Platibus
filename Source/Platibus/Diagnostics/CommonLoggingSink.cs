using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// A <see cref="IDiagnosticEventSink"/> implementation that sends formatted log messages to
    /// a Commons Logging log
    /// </summary>
    public class CommonLoggingSink : IDiagnosticEventSink
    {
        private readonly ILog _log;

        /// <summary>
        /// Initializes a new <see cref="CommonLoggingSink"/> targeting the specified 
        /// <paramref name="log"/>
        /// </summary>
        /// <param name="log">The log to target</param>
        public CommonLoggingSink(ILog log)
        {
            _log = log;
        }

        /// <inheritdoc />
        public Task Receive(DiagnosticEvent @event)
        {
            var message = FormatLogMessage(@event);
            switch (@event.Type.Level)
            {
                case DiagnosticEventLevel.Trace:
                    _log.Trace(message, @event.Exception);
                    break;
                case DiagnosticEventLevel.Debug:
                    _log.Debug(message, @event.Exception);
                    break;
                case DiagnosticEventLevel.Info:
                    _log.Info(message, @event.Exception);
                    break;
                case DiagnosticEventLevel.Warn:
                    _log.Warn(message, @event.Exception);
                    break;
                case DiagnosticEventLevel.Error:
                    _log.Error(message, @event.Exception);
                    break;
                default:
                    _log.Debug(message, @event.Exception);
                    break;
            }
            
            return Task.FromResult(0);
        }

        /// <summary>
        /// Formats the log message
        /// </summary>
        /// <param name="event">The diagnostic event</param>
        /// <returns>Returns a formatted log message</returns>
        protected virtual string FormatLogMessage(DiagnosticEvent @event)
        {
            var message = @event.Type.Name;
            if (@event.Detail != null)
            {
                message += ": " + @event.Detail;
            }

            var fields = new OrderedDictionary();
            if (@event.Message != null)
            {
                var headers = @event.Message.Headers;
                fields["MessageId"] = headers.MessageId;
                fields["Origination"] = headers.Origination;
                fields["Destination"] = headers.Destination;
            }
            fields["Queue"] = @event.Queue;
            fields["Topic"] = @event.Topic;

            var fieldsStr = string.Join("; ", fields
                .OfType<DictionaryEntry>()
                .Where(f => f.Value != null)
                .Select(f => f.Key + "=" + f.Value));

            if (!string.IsNullOrWhiteSpace(fieldsStr))
            {
                message += " (" + fieldsStr + ")";
            }
            return message;
        }
    }
}
