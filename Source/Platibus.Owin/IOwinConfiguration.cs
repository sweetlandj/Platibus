using System;
using Platibus.Config;
using Platibus.Security;

namespace Platibus.Owin
{
    public interface IOwinConfiguration : IPlatibusConfiguration
    {
        /// <summary>
        /// The URI on which the HTTP server should listen
        /// </summary>
        Uri BaseUri { get; }

        /// <summary>
        /// The subscription tracking service implementation
        /// </summary>
        ISubscriptionTrackingService SubscriptionTrackingService { get; }

        /// <summary>
        /// The message queueing service implementation
        /// </summary>
        IMessageQueueingService MessageQueueingService { get; }

        /// <summary>
        /// An optional component used to restrict access for callers to send
        /// messages or subscribe to topics
        /// </summary>
        IAuthorizationService AuthorizationService { get; }

        /// <summary>
        /// Whether the transport service can be bypassed when delivering messages
        /// whose destination and origination is the same.
        /// </summary>
        bool BypassTransportLocalDestination { get; }
    }
}
