using System;
using System.Net;
using Platibus.Config;
using Platibus.Security;

namespace Platibus.Http
{
    /// <summary>
    /// Configuration for hosting a Platibus instance in a standalone HTTP server
    /// </summary>
    public interface IHttpServerConfiguration : IPlatibusConfiguration
    {
        /// <summary>
        /// The URI on which the HTTP server should listen
        /// </summary>
        Uri BaseUri { get; }

        /// <summary>
        /// The authentication schemes that should be supported
        /// </summary>
        AuthenticationSchemes AuthenticationSchemes { get; }

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
        /// The maximum amount of HTTP requests to process at the same time.
        /// </summary>
        int ConcurrencyLimit { get; set; }

        /// <summary>
        /// Whether the transport service can be bypassed when delivering messages
        /// whose destination and origination is the same.
        /// </summary>
        bool BypassTransportLocalDestination { get; }
    }
}