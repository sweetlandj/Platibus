using System;
using System.Configuration;
using Platibus.Config;

namespace Platibus.Owin
{
    internal class OwinConfigurationSection : PlatibusConfigurationSection
    {
        private const string BaseUriPropertyName = "baseUri";
        private const string SubscriptionTrackingPropertyName = "subscriptionTracking";
        private const string QueueingPropertyName = "queueing";
        private const string BypassTransportLocalDestinationPropertyName = "bypassTransportLocalDestination";

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

        /// <summary>
        /// Whether the transport service can be bypassed when delivering messages
        /// whose destination and origination is the same.
        /// </summary>
        [ConfigurationProperty(BypassTransportLocalDestinationPropertyName, IsRequired = false, DefaultValue = false)]
        public bool BypassTransportLocalDestination
        {
            get { return (bool)base[BypassTransportLocalDestinationPropertyName]; }
            set { base[BypassTransportLocalDestinationPropertyName] = value; }
        }
    }
}