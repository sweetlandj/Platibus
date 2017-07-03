using Platibus.Diagnostics;

namespace Platibus.SQL
{
    /// <summary>
    /// Mutable factory object that produces immutable diagnostic events
    /// </summary>
    public class SQLEventBuilder : DiagnosticEventBuilder
    {
        /// <summary>
        /// The name of the connection, if applicable
        /// </summary>
        public string ConnectionName { get; set; }

        /// <summary>
        /// The query or command text, if applicable
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        /// Initializes a new <see cref="SQLEventBuilder"/> for the specified 
        /// </summary>
        /// <param name="source">The object that will produce the event</param>
        /// <param name="type">The type of event</param>
        public SQLEventBuilder(object source, DiagnosticEventType type) : base(source, type)
        {
        }

        /// <inheritdoc />
        public override DiagnosticEvent Build()
        {
            return new SQLEvent(Source, Type, Detail, Exception, Message, Endpoint, Queue, Topic, ConnectionName, CommandText);
        }
    }
}
