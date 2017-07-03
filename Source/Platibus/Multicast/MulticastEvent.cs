using System;
using Platibus.Diagnostics;

namespace Platibus.Multicast
{
    /// <summary>
    /// A diagnostic event related to UDP multicast
    /// </summary>
    public class MulticastEvent : DiagnosticEvent
    {
        private readonly NodeId _node;
        private readonly string _host;
        private readonly int? _port;

        /// <summary>
        /// The node to which the multicast event pertains
        /// </summary>
        /// <remarks>
        /// The node to which the event pertains may differ from the node that emitted the event
        /// </remarks>
        public NodeId Node { get { return _node; } }

        /// <summary>
        /// The host to which the event pertains
        /// </summary>
        /// <remarks>
        /// The host to which the event pertains may differ from the host that emitted the event
        /// </remarks>
        public string Host { get { return _host; } }

        /// <summary>
        /// The port to which the event pertains
        /// </summary>
        /// <remarks>
        /// The port to which the event pertains may differ from the port of the host that emitted
        /// the event
        /// </remarks>
        public int? Port { get { return _port; } }

        /// <summary>
        /// Initializes a new <see cref="MulticastEvent"/>
        /// </summary>
        /// <param name="source">The object that emitted the event</param>
        /// <param name="type">The type of event</param>
        /// <param name="detail">Specific details regarding this instance of the event</param>
        /// <param name="exception">The exception related to the event, if applicable</param>
        /// <param name="message">The message to which the event pertains, if applicable</param>
        /// <param name="endpointName">The name of the endpoint, if applicable</param>
        /// <param name="queue">The queue to which the event pertains, if applicable</param>
        /// <param name="topic">The topic to which the message pertains, if applicable</param>
        /// <param name="node">The node to which the multicast event pertains</param>
        /// <param name="host">The node to which the multicast event pertains</param>
        /// <param name="port">The node to which the multicast event pertains</param>
        public MulticastEvent(object source, DiagnosticEventType type, string detail = null, Exception exception = null, Message message = null, EndpointName endpointName = null, QueueName queue = null, TopicName topic = null, NodeId node = null, string host = null, int? port = null) 
            : base(source, type, detail, exception, message, endpointName, queue, topic)
        {
            _node = node;
            _host = host;
            _port = port;
        }
    }
}
