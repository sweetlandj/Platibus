using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Platibus.Filesystem;
using Platibus.Http;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// A base class for <see cref="IDiagnosticEventSink"/> implementations based on the Graylog
    /// Extended Log Format (GELF)
    /// </summary>
    public abstract class GelfLoggingSink : IDiagnosticEventSink
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        /// <inheritdoc />
        public void Receive(DiagnosticEvent @event)
        {
            try
            {
                var gelfMessage = new GelfMessage();
                PopulateGelfMessage(gelfMessage, @event);
                var json = JsonConvert.SerializeObject(gelfMessage, _jsonSerializerSettings);
                Process(json);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error processing GELF message: " + ex);
            }
        }

        /// <inheritdoc />
        public async Task ReceiveAsync(DiagnosticEvent @event, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var gelfMessage = new GelfMessage();
                PopulateGelfMessage(gelfMessage, @event);
                var json = JsonConvert.SerializeObject(gelfMessage, _jsonSerializerSettings);
                await ProcessAsync(json, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error processing GELF message: " + ex);
            }
        }

        /// <summary>
        /// Processes the specified GELF formatted string by writing it to a file or sending it to
        /// a network endpoint
        /// </summary>
        /// <param name="gelfMessage">The JSON serialized GELF message to process</param>
        /// <returns>Returns a task that will complete when the GELF message has been processed</returns>
        public abstract void Process(string gelfMessage);

        /// <summary>
        /// Processes the specified GELF formatted string by writing it to a file or sending it to
        /// a network endpoint
        /// </summary>
        /// <param name="gelfMessage">The JSON serialized GELF message to process</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be provided
        /// by the caller to interrupt processing of the GELF message</param>
        /// <returns>Returns a task that will complete when the GELF message has been processed</returns>
        public abstract Task ProcessAsync(string gelfMessage, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Serializes the specified <paramref name="gelfMessage"/> to a JSON string
        /// </summary>
        /// <param name="gelfMessage">The GELF message to serialize</param>
        /// <returns>Returns the JSON serialized <paramref name="gelfMessage"/></returns>
        protected string Serialize(GelfMessage gelfMessage)
        {
            if (gelfMessage == null) throw new ArgumentNullException("gelfMessage");
            return JsonConvert.SerializeObject(gelfMessage, _jsonSerializerSettings);
        }

        /// <summary>
        /// Sets the standard and common additional GELF fields with values from the specified
        /// diagnostic <paramref name="event"/>.
        /// </summary>
        /// <param name="gelfMessage">The GELF message to populate</param>
        /// <param name="event">The diagnostic event</param>
        /// <returns>The GELF formatted string</returns>
        protected virtual void PopulateGelfMessage(GelfMessage gelfMessage, DiagnosticEvent @event)
        {
            var source = @event.Source;
            gelfMessage.Facility = source == null ? "Platibus" : source.GetType().FullName;
            gelfMessage.Level = GetSyslogLevel(@event.Type.Level);
            gelfMessage.Timestamp = @event.Timestamp;
            gelfMessage.EventType = @event.Type;
            gelfMessage.Queue = @event.Queue;
            gelfMessage.Topic = @event.Topic;

            string shortMessage;
            if (MessageTooLong(@event.Detail, out shortMessage))
            {
                gelfMessage.ShortMessage = shortMessage;
                gelfMessage.FullMessage = @event.Detail;
            }
            else
            {
                gelfMessage.ShortMessage = @event.Detail;
            }

            if (@event.Exception != null)
            {
                gelfMessage.Exception = @event.Exception.ToString();
            }

            PopulateMessageFields(gelfMessage, @event);
            PopulateHttpFields(gelfMessage, @event as HttpEvent);
            PopulateFilesystemFields(gelfMessage, @event as FilesystemEvent);
        }

        protected virtual void PopulateMessageFields(GelfMessage gelfMessage, DiagnosticEvent @event)
        {
            if (@event.Message == null) return;

            var headers = @event.Message.Headers;
            gelfMessage.MessageId = headers.MessageId;
            gelfMessage.MessageName = headers.MessageName;
            gelfMessage.RelatedTo = headers.RelatedTo == default(MessageId)
                ? null
                : headers.MessageId.ToString();

            if (headers.Origination != null)
            {
                gelfMessage.Origination = headers.Origination.ToString();
            }
            if (headers.Destination != null)
            {
                gelfMessage.Destination = headers.Destination.ToString();
            }
            if (headers.ReplyTo != null)
            {
                gelfMessage.Destination = headers.ReplyTo.ToString();
            }
        }

        protected virtual void PopulateHttpFields(GelfMessage gelfMessage, HttpEvent httpEvent)
        {
            if (httpEvent == null) return;

            gelfMessage.Remote = httpEvent.Remote;
            gelfMessage.HttpMethod = httpEvent.Method;
            gelfMessage.HttpStatus = httpEvent.Status;
            if (httpEvent.Uri != null)
            {
                gelfMessage.Uri = httpEvent.Uri.ToString();
            }
        }

        protected virtual void PopulateFilesystemFields(GelfMessage gelfMessage, FilesystemEvent fsEvent)
        {
            if (fsEvent == null) return;
            if (fsEvent.File != null)
            {
                gelfMessage.File = fsEvent.File.FullName;
            }

            if (fsEvent.Directory != null)
            {
                gelfMessage.Directory = fsEvent.Directory.FullName;
            }
        }

        private static readonly IDictionary<DiagnosticEventLevel, int> SyslogLevels = new Dictionary<DiagnosticEventLevel, int>
        {
            { DiagnosticEventLevel.Trace, 7 },
            { DiagnosticEventLevel.Debug, 7 },
            { DiagnosticEventLevel.Info, 6 },
            { DiagnosticEventLevel.Warn, 4 },
            { DiagnosticEventLevel.Error, 3 }
        };

        /// <summary>
        /// Returns the syslog level corresponding to the specified diagnostic event 
        /// <paramref name="level"/>
        /// </summary>
        /// <param name="level">The diagnostic event level</param>
        /// <returns>Returns the syslog level corresponding to the specified diagnostic event
        /// level</returns>
        protected int GetSyslogLevel(DiagnosticEventLevel level)
        {
            int syslogLevel;
            return SyslogLevels.TryGetValue(level, out syslogLevel) ? syslogLevel : 7;
        }

        /// <summary>
        /// Determines whether the <paramref name="fullMessage"/> is too long to use for the GELF
        /// <c>short_message</c> field, parsing out an appropriate substring to use as the short 
        /// message that is the case.
        /// </summary>
        /// <param name="fullMessage">The full message</param>
        /// <param name="shortMessage">A variable that will receive a substring of the
        /// <paramref name="fullMessage"/> that is appropriate to use for the <c>short_message</c>
        /// if the <paramref name="fullMessage"/> is too long</param>
        /// <returns>Returns <c>true</c> if the <paramref name="fullMessage"/> is too long and
        /// a separate <paramref name="shortMessage"/> is needed; <c>false</c> otherwise</returns>
        protected virtual bool MessageTooLong(string fullMessage, out string shortMessage)
        {
            if (string.IsNullOrWhiteSpace(fullMessage))
            {
                shortMessage = fullMessage;
                return false;
            }

            const int maxShortMessageLength = 100;
            const string trailingEllipses = "...";
            if (fullMessage.Length > maxShortMessageLength)
            {
                var cutoffPoint = maxShortMessageLength - trailingEllipses.Length;
                var lastWhitespace = fullMessage.LastIndexOf(" ", cutoffPoint, StringComparison.OrdinalIgnoreCase);
                if (lastWhitespace > 0)
                {
                    cutoffPoint = lastWhitespace;
                }
                shortMessage = fullMessage.Substring(0, cutoffPoint) + trailingEllipses;
                return true;
            }

            shortMessage = fullMessage;
            return false;
        }
    }
}