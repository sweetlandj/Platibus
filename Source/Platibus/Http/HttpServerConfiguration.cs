using System;
using System.Net;
using Platibus.Config;
using Platibus.InMemory;

namespace Platibus.Http
{
    public class HttpServerConfiguration : PlatibusConfiguration, IHttpServerConfiguration
    {
        private Uri _baseUri;
        private AuthenticationSchemes _authenticationSchemes = AuthenticationSchemes.Anonymous;

        public Uri BaseUri
        {
            get { return _baseUri ?? (_baseUri = new Uri("http://localhost/platibus")); }
            set { _baseUri = value; }
        }

        public AuthenticationSchemes AuthenticationSchemes
        {
            get { return _authenticationSchemes; }
            set { _authenticationSchemes = value; }
        }

        public ISubscriptionTrackingService SubscriptionTrackingService { get; set; }
        public IMessageQueueingService MessageQueueingService { get; set; }

        public HttpServerConfiguration()
        {
            SubscriptionTrackingService = new InMemorySubscriptionTrackingService();
            MessageQueueingService = new InMemoryMessageQueueingService();
        }
    }
}