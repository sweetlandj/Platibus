using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// A <see cref="IDiagnosticService"/> implementation that sends formatted log messages to
    /// a Commons Logging log
    /// </summary>
    public class CommonLoggingSink : IDiagnosticEventSink
    {
        private readonly Func<DiagnosticEvent, ILog> _logFactory;

        /// <summary>
        /// Initializes a new <see cref="CommonLoggingSink"/> with default log factory
        /// </summary>
        public CommonLoggingSink()
        {
            _logFactory = DefaultLogFactory;
        }

        /// <summary>
        /// Initializes a new <see cref="CommonLoggingSink"/> that directs all output to the
        /// specified <paramref name="log"/>
        /// </summary>
        /// <param name="log">The log to which all messages should be directed</param>
        public CommonLoggingSink(ILog log)
        {
            if (log == null)
            {
                _logFactory = DefaultLogFactory;
            }
            else
            {
                _logFactory = _ => log;
            }
        }

        /// <summary>
        /// Initializes a new <see cref="CommonLoggingSink"/> that uses the supplied 
        /// <paramref name="logFactory"/> function to determine the appropriate <see cref="ILog"/>
        /// to use for each diagnostic event that is received.
        /// </summary>
        /// <param name="logFactory">A function that returns the appropriate log to use for
        /// each diagnostic event</param>
        public CommonLoggingSink(Func<DiagnosticEvent, ILog> logFactory)
        {
            _logFactory = logFactory ?? DefaultLogFactory;
        }

        private static ILog DefaultLogFactory(DiagnosticEvent @event)
        {
            var source = @event.Source;
            var category = source == null ? "Platibus" : source.GetType().FullName;
            var log = LogManager.GetLogger(category);
            return log;
        }

        /// <inheritdoc />
        public void Consume(DiagnosticEvent @event)
        {
            var log = _logFactory(@event);

            var message = FormatLogMessage(@event);
            switch (@event.Type.Level)
            {
                case DiagnosticEventLevel.Trace:
                    log.Trace(message, @event.Exception);
                    break;
                case DiagnosticEventLevel.Debug:
                    log.Debug(message, @event.Exception);
                    break;
                case DiagnosticEventLevel.Info:
                    log.Info(message, @event.Exception);
                    break;
                case DiagnosticEventLevel.Warn:
                    log.Warn(message, @event.Exception);
                    break;
                case DiagnosticEventLevel.Error:
                    log.Error(message, @event.Exception);
                    break;
                default:
                    log.Debug(message, @event.Exception);
                    break;
            }
        }

        /// <inheritdoc />
        public Task ConsumeAsync(DiagnosticEvent @event, CancellationToken cancellationToken = new CancellationToken())
        {
            Consume(@event);
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
