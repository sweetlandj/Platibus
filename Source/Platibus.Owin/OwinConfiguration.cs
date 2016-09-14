using System;
using Platibus.Config;
using Platibus.InMemory;
using Platibus.Security;

namespace Platibus.Owin
{
    /// <summary>
    /// Extends the base <see cref="PlatibusConfiguration"/> with Owin-specific configuration
    /// </summary>
    public class OwinConfiguration : PlatibusConfiguration, IOwinConfiguration
    {
        private Uri _baseUri;

        /// <summary>
        /// The URI on which the HTTP server should listen
        /// </summary>
        public Uri BaseUri
        {
            get { return _baseUri ?? (_baseUri = new Uri("http://localhost/platibus")); }
            set { _baseUri = value; }
        }

        /// <summary>
        /// The subscription tracking service implementation
        /// </summary>
        public ISubscriptionTrackingService SubscriptionTrackingService { get; set; }

        /// <summary>
        /// The message queueing service implementation
        /// </summary>
        public IMessageQueueingService MessageQueueingService { get; set; }

        /// <summary>
        /// An optional component used to restrict access for callers to send
        /// messages or subscribe to topics
        /// </summary>
        public IAuthorizationService AuthorizationService { get; set; }

        /// <summary>
        /// Whether the transport service can be bypassed when delivering messages
        /// whose destination and origination is the same.
        /// </summary>
        public bool BypassTransportLocalDestination { get; set; }

        /// <summary>
        /// Initializes a new <see cref="OwinConfiguration"/> with defaults
        /// </summary>
        public OwinConfiguration()
        {
            SubscriptionTrackingService = new InMemorySubscriptionTrackingService();
            MessageQueueingService = new InMemoryMessageQueueingService();
        }
    }
}