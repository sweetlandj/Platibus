using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// A <see cref="IDiagnosticEventSink"/> implementation that writes log messages to a 
    /// <see cref="TextWriter"/>
    /// </summary>
    public class TextLoggingSink : IDiagnosticEventSink
    {
        private readonly TextWriter _writer;

        /// <summary>
        /// Initializes a new <see cref="TextLoggingSink"/> that targets the supplied
        /// text <paramref name="writer"/>
        /// </summary>
        /// <param name="writer">The text writer to target</param>
        public TextLoggingSink(TextWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            _writer = writer;
        }

        /// <inheritdoc />
        public void Consume(DiagnosticEvent @event)
        {
            _writer.WriteLine(FormatLogMessage(@event));
        }

        /// <inheritdoc />
        public Task ConsumeAsync(DiagnosticEvent @event, CancellationToken cancellationToken = new CancellationToken())
        {
            return _writer.WriteLineAsync(FormatLogMessage(@event));
        }

        /// <summary>
        /// Formats the log message
        /// </summary>
        /// <param name="event">The diagnostic event</param>
        /// <returns>Returns a formatted log message</returns>
        protected virtual string FormatLogMessage(DiagnosticEvent @event)
        {
            var source = @event.Source;
            var timestamp = @event.Timestamp.ToString("s");
            var level = @event.Type.Level.ToString("G");
            var category = source == null ? "Platibus" : source.GetType().FullName;
            var message = timestamp + " " + level + " " + category + " " + @event.Type.Name;
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