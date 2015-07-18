using System;
using System.Configuration;
using Platibus.Config;

namespace Platibus.IIS
{
    internal class IISConfigurationSection : PlatibusConfigurationSection
    {
        private const string BaseUriPropertyName = "baseUri";
        private const string SubscriptionTrackingPropertyName = "subscriptionTracking";
        private const string QueueingPropertyName = "queueing";

        [ConfigurationProperty(BaseUriPropertyName)]
        public Uri BaseUri
        {
            get { return (Uri) base[BaseUriPropertyName]; }
            set { base[BaseUriPropertyName] = value; }
        }

        [ConfigurationProperty(SubscriptionTrackingPropertyName)]
        public SubscriptionTrackingElement SubscriptionTracking
        {
            get { return (SubscriptionTrackingElement) base[SubscriptionTrackingPropertyName]; }
            set { base[SubscriptionTrackingPropertyName] = value; }
        }

        [ConfigurationProperty(QueueingPropertyName)]
        public QueueingElement Queueing
        {
            get { return (QueueingElement) base[QueueingPropertyName]; }
            set { base[QueueingPropertyName] = value; }
        }
    }
}