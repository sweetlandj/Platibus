using System;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// Mutable factory object that produces immutable diagnostic events
    /// </summary>
    public class DiagnosticEventBuilder
    {
        /// <summary>
        /// The object that will produce the event
        /// </summary>
        protected readonly object Source;

        /// <summary>
        /// The type of event
        /// </summary>
        protected readonly DiagnosticEventType Type;

        /// <summary>
        /// Initializes a new <see cref="DiagnosticEventBuilder"/> for the specified 
        /// </summary>
        /// <param name="source">The object that will produce the event</param>
        /// <param name="type">The type of event</param>
        public DiagnosticEventBuilder(object source, DiagnosticEventType type)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (type == null) throw new ArgumentNullException("type");
            Source = source;
            Type = type;
        }

        /// <summary>
        /// Specific details regarding this instance of the event
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// The exception related to the event, if applicable
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// The message to which the event pertains, if applicable
        /// </summary>
        public Message Message { get; set; }

        /// <summary>
        /// The queue to which the message pertains, if applicable
        /// </summary>
        public QueueName Queue { get; set; }

        /// <summary>
        /// The topic to which the message pertains, if applicable
        /// </summary>
        public TopicName Topic { get; set; }

        /// <summary>
        /// Builds the diagnostic event
        /// </summary>
        /// <returns>Returns the newly constructed diagnostic event</returns>
        public virtual DiagnosticEvent Build()
        {
            return new DiagnosticEvent(Source, Type, Detail, Exception, Message, Queue, Topic);
        }
    }
}
