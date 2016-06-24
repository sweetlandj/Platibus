using System;
using System.Net;
using Platibus.Config;
using Platibus.InMemory;
using Platibus.Security;

namespace Platibus.Http
{
    /// <summary>
    /// Configuration for hosting Platibus in a standalone HTTP server
    /// </summary>
    public class HttpServerConfiguration : PlatibusConfiguration, IHttpServerConfiguration
    {
        private Uri _baseUri;
        private AuthenticationSchemes _authenticationSchemes = AuthenticationSchemes.Anonymous;

        /// <summary>
        /// The URI on which the HTTP server should listen
        /// </summary>
        public Uri BaseUri
        {
            get { return _baseUri ?? (_baseUri = new Uri("http://localhost/platibus")); }
            set { _baseUri = value; }
        }

        /// <summary>
        /// The authentication schemes that should be supported
        /// </summary>
        public AuthenticationSchemes AuthenticationSchemes
        {
            get { return _authenticationSchemes; }
            set { _authenticationSchemes = value; }
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
        /// The maximum amount of HTTP requests to process at the same time.
        /// </summary>
        public int ConcurrencyLimit { get; set; }

        /// <summary>
        /// Whether the transport service can be bypassed when delivering messages
        /// whose destination and origination is the same.
        /// </summary>
        public bool BypassTransportLocalDestination { get; set; }

        /// <summary>
        /// Initializes a new <see cref="HttpServerConfiguration"/> with defaults
        /// </summary>
        public HttpServerConfiguration()
        {
            SubscriptionTrackingService = new InMemorySubscriptionTrackingService();
            MessageQueueingService = new InMemoryMessageQueueingService();
        }
    }
}