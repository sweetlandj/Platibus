using System;
using System.Net;
using Platibus.Config;
using Platibus.InMemory;

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
        /// Initializes a new <see cref="HttpServerConfiguration"/> with defaults
        /// </summary>
        public HttpServerConfiguration()
        {
            SubscriptionTrackingService = new InMemorySubscriptionTrackingService();
            MessageQueueingService = new InMemoryMessageQueueingService();
        }
    }
}