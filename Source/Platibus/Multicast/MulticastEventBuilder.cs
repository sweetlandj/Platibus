using Platibus.Diagnostics;

namespace Platibus.Multicast
{
    /// <summary>
    /// Mutable factory object that produces immutable diagnostic events[
    /// </summary>
    public class MulticastEventBuilder : DiagnosticEventBuilder
    {
        /// <summary>
        /// The node to which the multicast event pertains
        /// </summary>
        /// <remarks>
        /// The node to which the event pertains may differ from the node that emitted the event
        /// </remarks>
        public NodeId Node { get; set; }

        /// <summary>
        /// The host to which the event pertains
        /// </summary>
        /// <remarks>
        /// The host to which the event pertains may differ from the host that emitted the event
        /// </remarks>
        public string Host { get; set; }

        /// <summary>
        /// The port to which the event pertains
        /// </summary>
        /// <remarks>
        /// The port to which the event pertains may differ from the port of the host that emitted
        /// the event
        /// </remarks>
        public int? Port { get; set; }

        /// <summary>
        /// Initializes a new <see cref="DiagnosticEventBuilder"/> for the specified 
        /// </summary>
        /// <param name="source">The object that will produce the event</param>
        /// <param name="type">The type of event</param>
        public MulticastEventBuilder(object source, DiagnosticEventType type) : base(source, type)
        {
        }

        /// <inheritdoc />
        public override DiagnosticEvent Build()
        {
            return new MulticastEvent(Source, Type, Detail, Exception, Message, Endpoint, Queue, Topic, Node, Host, Port);
        }
    }
}
