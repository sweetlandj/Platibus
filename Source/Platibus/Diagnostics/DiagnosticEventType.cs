// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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
        /// Emitted as a component or service is initialized
        /// </summary>
        public static readonly DiagnosticEventType ComponentInitialization = new DiagnosticEventType("ComponentInitialization", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted when a default configuration is used
        /// </summary>
        public static readonly DiagnosticEventType ConfigurationDefault = new DiagnosticEventType("ConfigurationDefault", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted when an invalid configuration is encountered
        /// </summary>
        public static readonly DiagnosticEventType ConfigurationError = new DiagnosticEventType("ConfigurationError", DiagnosticEventLevel.Error);

        /// <summary>
        /// Emitted when a configuration hook is processed
        /// </summary>
        public static readonly DiagnosticEventType ConfigurationHook = new DiagnosticEventType("ConfigurationHook", DiagnosticEventLevel.Error);

        /// <summary>
        /// Emitted whenever a bus instance is initialized
        /// </summary>
        public static readonly DiagnosticEventType BusInitialized = new DiagnosticEventType("BusInitialized", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted whenever an attempt is made to access a bus instance that has not been fully 
        /// initialized
        /// </summary>
        public static readonly DiagnosticEventType BusNotInitialized = new DiagnosticEventType("BusNotInitialized", DiagnosticEventLevel.Error);
        
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
        /// Emitted whenever a message is successfully delivered to its intended destination
        /// </summary>
        public static readonly DiagnosticEventType MessageDelivered = new DiagnosticEventType("MessageDelivered", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted whenever a message cannot be delivered to its intended destination
        /// </summary>
        public static readonly DiagnosticEventType MessageDeliveryFailed = new DiagnosticEventType("MessageDeliveryFailed", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted whenever a message is published to a topic
        /// </summary>
        public static readonly DiagnosticEventType MessagePublished = new DiagnosticEventType("MessagePublished", DiagnosticEventLevel.Trace);
        
        /// <summary>
        /// Emitted whenever a message is inserted into a message queue
        /// </summary>
        public static readonly DiagnosticEventType MessageEnqueued = new DiagnosticEventType("MessageEnqueued", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted whenever a message is added back into a queue e.g. after a restart
        /// </summary>
        public static readonly DiagnosticEventType MessageRequeued = new DiagnosticEventType("MessageRequeued", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted whenever an attempt is made to process a queued message
        /// </summary>
        public static readonly DiagnosticEventType QueuedMessageAttempt = new DiagnosticEventType("QueuedMessageAttempt", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted whenever a queued message will be retried
        /// </summary>
        public static readonly DiagnosticEventType QueuedMessageRetry = new DiagnosticEventType("QueuedMessageRetry", DiagnosticEventLevel.Trace);
        
        /// <summary>
        /// Emitted whenever a message is acknowledged (i.e. successfully handled)
        /// </summary>
        public static readonly DiagnosticEventType MessageAcknowledged = new DiagnosticEventType("MessageAcknowledged", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted whenever an attempt is made to sent a message or subscription request to a
        /// named endpoint that has not been configured
        /// </summary>
        public static readonly DiagnosticEventType EndpointNotFound = new DiagnosticEventType("EndpointNotFound", DiagnosticEventLevel.Error);

        /// <summary>
        /// Emitted whenever a message is not acknowledged
        /// </summary>
        public static readonly DiagnosticEventType MessageNotAcknowledged = new DiagnosticEventType("MessageNotAcknowledged", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted whenever an attempt is made to handle a message that has expired
        /// </summary>
        public static readonly DiagnosticEventType MessageExpired = new DiagnosticEventType("MessageExpired", DiagnosticEventLevel.Warn);

        /// <summary>
        /// Emitted whenever the maximum number of attempts to process a message have been exceeded
        /// </summary>
        public static readonly DiagnosticEventType MaxAttemptsExceeded = new DiagnosticEventType("MaxAttemptsExceeded", DiagnosticEventLevel.Warn);

        /// <summary>
        /// Emitted whenever a message is abandoned (i.e. moved to a dead letter queue) due to
        /// there not being any handling rules that match the message or due to the message not
        /// being acknowledged by any handlers
        /// </summary>
        public static readonly DiagnosticEventType DeadLetter = new DiagnosticEventType("DeadLetter", DiagnosticEventLevel.Warn);
        
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
        public static readonly DiagnosticEventType MessageSendFailed = new DiagnosticEventType("SendFailed", DiagnosticEventLevel.Error);

        /// <summary>
        /// Emitted whenever a subscription request attempt fails
        /// </summary>
        public static readonly DiagnosticEventType SubscriptionFailed = new DiagnosticEventType("SubscriptionFailed", DiagnosticEventLevel.Error);

        /// <summary>
        /// Emitted whenever a subscription is renewed
        /// </summary>
        public static readonly DiagnosticEventType SubscriptionRenewed = new DiagnosticEventType("SubscriptionRenewed", DiagnosticEventLevel.Trace);

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

        /// <summary>
        /// Emitted when an unexpected exception is caught
        /// </summary>
        public static readonly DiagnosticEventType UnhandledException = new DiagnosticEventType("UnhandledException", DiagnosticEventLevel.Error);
        
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
