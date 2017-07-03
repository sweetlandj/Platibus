using Platibus.Diagnostics;

namespace Platibus.Multicast
{
    /// <summary>
    /// Types of <see cref="DiagnosticEvent"/>s specific to UDP multicast
    /// </summary>
    public class MulticastEventType
    {
        /// <summary>
        /// Emitted when a broadcast socket is bound
        /// </summary>
        public static readonly DiagnosticEventType BroadcastSocketBound = new DiagnosticEventType("BroadcastSocketBound", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted when a listener socket is bound
        /// </summary>
        public static readonly DiagnosticEventType ListenerSocketBound = new DiagnosticEventType("ListenerSocketBound", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted when a listener is started
        /// </summary>
        public static readonly DiagnosticEventType ListenerStarted = new DiagnosticEventType("ListenerStarted", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted when a listener is stopped
        /// </summary>
        public static readonly DiagnosticEventType ListenerStopped = new DiagnosticEventType("ListenerStopped", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted when a datagram is received
        /// </summary>
        public static readonly DiagnosticEventType DatagramReceived = new DiagnosticEventType("DatagramReceived", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted when a datagram is broadcast to the multicast group
        /// </summary>
        public static readonly DiagnosticEventType DatagramBroadcast = new DiagnosticEventType("DatagramBroadcast", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted when a the received datagram is not correctly formatted
        /// </summary>
        public static readonly DiagnosticEventType MalformedDatagram = new DiagnosticEventType("MalformedDatagram", DiagnosticEventLevel.Trace);
    }
}
