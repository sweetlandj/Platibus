using System;
using System.Net;
using Platibus.Config;

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
    }
}