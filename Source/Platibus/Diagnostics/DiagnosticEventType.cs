using System;
using System.Diagnostics;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// Denotes a type or category of diagnostic event
    /// </summary>
    [DebuggerDisplay("{_name,nq}")]
    public class DiagnosticEventType
    {
        /// <summary>
        /// Emitted whenever a bus instance is initialized
        /// </summary>
        public static readonly DiagnosticEventType BusInitialized = new DiagnosticEventType("BusInitialized", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted whenever a bus instance is shut down
        /// </summary>
        public static readonly DiagnosticEventType BusShutDown = new DiagnosticEventType("BusShutDown", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted whenever a message is received
        /// </summary>
        public static readonly DiagnosticEventType MessageReceived = new DiagnosticEventType("MessageReceived", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted whenever a message is sent
        /// </summary>
        public static readonly DiagnosticEventType MessageSent = new DiagnosticEventType("MessageSent", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted whenever a message is published to a topic
        /// </summary>
        public static readonly DiagnosticEventType MessagePublished = new DiagnosticEventType("MessagePublished", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted whenever a message is acknowledged (i.e. successfully handled)
        /// </summary>
        public static readonly DiagnosticEventType MessageAcknowledged = new DiagnosticEventType("MessageAcknowledged", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted whenever a message is abandoned (i.e. moved to a dead letter queue) due to
        /// their not being any handling rules that match the message or due to the message not
        /// being acknowledged by any handlers
        /// </summary>
        public static readonly DiagnosticEventType MessageAbandoned = new DiagnosticEventType("MessageAbandoned", DiagnosticEventLevel.Warn);

        /// <summary>
        /// Emitted whenever a message is inserted into a message queue
        /// </summary>
        public static readonly DiagnosticEventType MessageEnqueued = new DiagnosticEventType("MessageEnqueued", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted whenever a message is received out of a message queue for processing
        /// </summary>
        public static readonly DiagnosticEventType MessageDequeued = new DiagnosticEventType("MessageDequeued", DiagnosticEventLevel.Trace);
        
        /// <summary>
        /// Emitted whenever access is denied to a resource
        /// </summary>
        public static readonly DiagnosticEventType AccessDenied = new DiagnosticEventType("AccessDenied", DiagnosticEventLevel.Warn);

        /// <summary>
        /// Emitted whenever no matching message handling rules are found
        /// </summary>
        public static readonly DiagnosticEventType NoMatchingHandlingRules = new DiagnosticEventType("NoMatchingHandlingRules", DiagnosticEventLevel.Warn);

        /// <summary>
        /// Emitted whenever no matching send rules are found
        /// </summary>
        public static readonly DiagnosticEventType NoMatchingSendRules = new DiagnosticEventType("NoMatchingSendRules", DiagnosticEventLevel.Warn);

        /// <summary>
        /// Emitted whenever an attempt to send a message fails
        /// </summary>
        public static readonly DiagnosticEventType SendFailed = new DiagnosticEventType("SendFailed", DiagnosticEventLevel.Error);

        /// <summary>
        /// Emitted whenever a subscription request attempt fails
        /// </summary>
        public static readonly DiagnosticEventType SubscriptionFailed = new DiagnosticEventType("SubscriptionFailed", DiagnosticEventLevel.Error);

        /// <summary>
        /// Emitted whenever a subscription is created or updated in the local subscription
        /// tracking service
        /// </summary>
        public static readonly DiagnosticEventType SubscriptionUpdated = new DiagnosticEventType("SubscriptionUpdated", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted whenever a reply message cannot be correlated to the original sent message
        /// </summary>
        /// <remarks>
        /// This could be due to an overly aggressive reply timeout or a reply being routed to
        /// the wrong instance due to a misconfigured load balancer or base URI
        /// </remarks>
        public static readonly DiagnosticEventType CorrelationFailed = new DiagnosticEventType("CorrelationFailed", DiagnosticEventLevel.Warn);

        private readonly string _name;
        private readonly DiagnosticEventLevel _level;

        /// <summary>
        /// The name of the diagnostic event type
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// An indication of the purpose and relative importance of this event
        /// </summary>
        public DiagnosticEventLevel Level { get { return _level; } }

        /// <summary>
        /// Initializes a new <see cref="DiagnosticEventType"/> with the specified 
        /// <paramref name="value"/>
        /// </summary>
        /// <param name="value">The name of the event type</param>
        /// <param name="level">An indication of the purpose and relative importance of this event</param>
        public DiagnosticEventType(string value, DiagnosticEventLevel level = DiagnosticEventLevel.Debug)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("value");
            _name = value.Trim();
            _level = level;
        }
        
        /// <inheritdoc />
        public override string ToString()
        {
            return _name;
        }

        /// <summary>
        /// Determines whether another <paramref name="eventType"/> is equal to this one
        /// </summary>
        /// <param name="eventType">The other event type</param>
        /// <returns>Returns <c>true</c> if the other <paramref name="eventType"/> is equal
        /// to this one; <c>false</c> otherwise</returns>
        public bool Equals(DiagnosticEventType eventType)
        {
            return !ReferenceEquals(null, eventType) &&
                   string.Equals(_name, eventType._name, StringComparison.InvariantCultureIgnoreCase);
        }
        
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as DiagnosticEventType);
        }
        
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _name.ToLowerInvariant().GetHashCode();
        }

        /// <summary>
        /// Overloaded equality operator used to determine whether two <see cref="DiagnosticEventType"/>
        /// objects are equal
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>Returns <c>true</c> if the two eventType names are equal; <c>false</c>
        /// otherwise</returns>
        public static bool operator ==(DiagnosticEventType left, DiagnosticEventType right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Overloaded inequality operator used to determine whether two <see cref="DiagnosticEventType"/>
        /// objects are not equal
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>Returns <c>true</c> if the two eventType names are not equal; <c>false</c>
        /// otherwise</returns>
        public static bool operator !=(DiagnosticEventType left, DiagnosticEventType right)
        {
            return !Equals(left, right);
        }
        
        /// <summary>
        /// Implicitly converts a <see cref="DiagnosticEventType"/> object to a string
        /// </summary>
        /// <param name="eventType">The eventType name object</param>
        /// <returns>Returns <c>null</c> if <paramref name="eventType"/> is <c>null</c>;
        /// otherwise returns the value of the <paramref name="eventType"/> object</returns>
        public static implicit operator string(DiagnosticEventType eventType)
        {
            return eventType == null ? null : eventType._name;
        }
    }
}
