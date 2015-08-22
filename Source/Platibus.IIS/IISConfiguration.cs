using System;
using Platibus.Config;
using Platibus.InMemory;

namespace Platibus.IIS
{
    /// <summary>
    /// Extends the base <see cref="PlatibusConfiguration"/> with IIS-specific configuration
    /// </summary>
    public class IISConfiguration : PlatibusConfiguration, IIISConfiguration
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
        /// Initializes a new <see cref="IISConfiguration"/> with defaults
        /// </summary>
        public IISConfiguration()
        {
            SubscriptionTrackingService = new InMemorySubscriptionTrackingService();
            MessageQueueingService = new InMemoryMessageQueueingService();
        }
    }
}