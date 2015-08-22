using System;
using Platibus.Config;

namespace Platibus.IIS
{
    /// <summary>
    /// Extends the base <see cref="IPlatibusConfiguration"/> with IIS-specific configuration
    /// </summary>
    public interface IIISConfiguration : IPlatibusConfiguration
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
    }
}