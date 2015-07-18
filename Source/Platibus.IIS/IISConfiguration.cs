using System;
using Platibus.Config;
using Platibus.InMemory;

namespace Platibus.IIS
{
    public class IISConfiguration : PlatibusConfiguration, IIISConfiguration
    {
        private Uri _baseUri;

        public Uri BaseUri
        {
            get { return _baseUri ?? (_baseUri = new Uri("http://localhost/platibus")); }
            set { _baseUri = value; }
        }

        public ISubscriptionTrackingService SubscriptionTrackingService { get; set; }
        public IMessageQueueingService MessageQueueingService { get; set; }

        public IISConfiguration()
        {
            SubscriptionTrackingService = new InMemorySubscriptionTrackingService();
            MessageQueueingService = new InMemoryMessageQueueingService();
        }
    }
}