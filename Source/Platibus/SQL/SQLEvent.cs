using System;
using Platibus.Diagnostics;

namespace Platibus.SQL
{
    /// <summary>
    /// A diagnostic event related to a SQL/ADO.NET operation
    /// </summary>
    public class SQLEvent : DiagnosticEvent
    {
        private readonly string _connectionName;
        private readonly string _commandText;

        /// <summary>
        /// The name of the connection, if applicable
        /// </summary>
        public string ConnectionName { get { return _connectionName; } }

        /// <summary>
        /// The query or command text, if applicable
        /// </summary>
        public string CommandText { get { return _commandText; } }

        /// <summary>
        /// Initializes a new SQL diagnostic event
        /// </summary>
        /// <param name="source">The object that emitted the event</param>
        /// <param name="type">The type of event</param>
        /// <param name="detail">Specific details regarding this instance of the event</param>
        /// <param name="exception">The exception related to the event, if applicable</param>
        /// <param name="message">The message to which the event pertains, if applicable</param>
        /// <param name="endpoint">The name of the endpoint, if applicable</param>
        /// <param name="queue">The queue to which the event pertains, if applicable</param>
        /// <param name="topic">The topic to which the message pertains, if applicable</param>
        /// <param name="connectionName">The name of the connection, if applicable</param>
        /// <param name="commandText">The query string, if applicable</param>
        public SQLEvent(object source, DiagnosticEventType type, string detail = null, Exception exception = null, Message message = null, EndpointName endpoint = null, QueueName queue = null, TopicName topic = null, string connectionName = null, string commandText = null) 
            : base(source, type, detail, exception, message, endpoint, queue, topic)
        {
            _connectionName = connectionName;
            _commandText = commandText;
        }
    }
}
