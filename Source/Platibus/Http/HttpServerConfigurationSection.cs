using System;
using System.Configuration;
using Platibus.Config;

namespace Platibus.Http
{
    /// <summary>
    /// A configuration section for hosting a Platibus instance in a standalone HTTP server
    /// </summary>
    /// <seealso cref="IHttpServerConfiguration"/>
    /// <seealso cref="HttpServerConfigurationManager"/>
    /// <seealso cref="HttpServer"/>
    public class HttpServerConfigurationSection : PlatibusConfigurationSection
    {
        private const string BaseUriPropertyName = "baseUri";
        private const string ConcurrencyLimitPropertyName = "concurrencyLimit";
        private const string AuthenticationSchemesPropertyName = "authenticationSchemes";
        private const string SubscriptionTrackingPropertyName = "subscriptionTracking";
        private const string QueueingPropertyName = "queueing";

        /// <summary>
        /// The base URI for the HTTP server (i.e. the address to which the HTTP
        /// server will listen)
        /// </summary>
        [ConfigurationProperty(BaseUriPropertyName)]
        public Uri BaseUri
        {
            get { return (Uri) base[BaseUriPropertyName]; }
            set { base[BaseUriPropertyName] = value; }
        }

        /// <summary>
        /// The maximum number of HTTP requests to process at the same time.
        /// </summary>
        [ConfigurationProperty(ConcurrencyLimitPropertyName)]
        public int ConcurrencyLimit
        {
            get { return (int)base[ConcurrencyLimitPropertyName]; }
            set { base[ConcurrencyLimitPropertyName] = value; }
        }

        /// <summary>
        /// The authentication schemes to support
        /// </summary>
        [ConfigurationProperty(AuthenticationSchemesPropertyName)]
        public AuthenticationSchemesElementCollection AuthenticationSchemes
        {
            get { return (AuthenticationSchemesElementCollection) base[AuthenticationSchemesPropertyName]; }
            set { base[AuthenticationSchemesPropertyName] = value; }
        }

        /// <summary>
        /// Configuration related to tracking subscriptions
        /// </summary>
        [ConfigurationProperty(SubscriptionTrackingPropertyName)]
        public SubscriptionTrackingElement SubscriptionTracking
        {
            get { return (SubscriptionTrackingElement) base[SubscriptionTrackingPropertyName]; }
            set { base[SubscriptionTrackingPropertyName] = value; }
        }

        /// <summary>
        /// Configuration related to message queueing
        /// </summary>
        [ConfigurationProperty(QueueingPropertyName)]
        public QueueingElement Queueing
        {
            get { return (QueueingElement) base[QueueingPropertyName]; }
            set { base[QueueingPropertyName] = value; }
        }
    }
}