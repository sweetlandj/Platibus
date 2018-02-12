#if NET452 || NET461
// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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
        private const string BypassTransportLocalDestinationPropertyName = "bypassTransportLocalDestination";

        /// <summary>
        /// The base URI for the HTTP server (i.e. the address to which the HTTP
        /// server will listen)
        /// </summary>
        [ConfigurationProperty(BaseUriPropertyName)]
        public Uri BaseUri
        {
            get => (Uri) base[BaseUriPropertyName];
            set => base[BaseUriPropertyName] = value;
        }

        /// <summary>
        /// The maximum number of HTTP requests to process at the same time.
        /// </summary>
        [ConfigurationProperty(ConcurrencyLimitPropertyName)]
        public int ConcurrencyLimit
        {
            get => (int)base[ConcurrencyLimitPropertyName];
            set => base[ConcurrencyLimitPropertyName] = value;
        }

        /// <summary>
        /// The authentication schemes to support
        /// </summary>
        [ConfigurationProperty(AuthenticationSchemesPropertyName)]
        public AuthenticationSchemesElementCollection AuthenticationSchemes
        {
            get => (AuthenticationSchemesElementCollection) base[AuthenticationSchemesPropertyName];
            set => base[AuthenticationSchemesPropertyName] = value;
        }

        /// <summary>
        /// Configuration related to tracking subscriptions
        /// </summary>
        [ConfigurationProperty(SubscriptionTrackingPropertyName)]
        public SubscriptionTrackingElement SubscriptionTracking
        {
            get => (SubscriptionTrackingElement) base[SubscriptionTrackingPropertyName];
            set => base[SubscriptionTrackingPropertyName] = value;
        }

        /// <summary>
        /// Configuration related to message queueing
        /// </summary>
        [ConfigurationProperty(QueueingPropertyName)]
        public QueueingElement Queueing
        {
            get => (QueueingElement) base[QueueingPropertyName];
            set => base[QueueingPropertyName] = value;
        }

        /// <summary>
        /// Whether the transport service can be bypassed when delivering messages
        /// whose destination and origination is the same.
        /// </summary>
        [ConfigurationProperty(BypassTransportLocalDestinationPropertyName, IsRequired = false, DefaultValue = false)]
        public bool BypassTransportLocalDestination
        {
            get => (bool)base[BypassTransportLocalDestinationPropertyName];
            set => base[BypassTransportLocalDestinationPropertyName] = value;
        }
    }
}
#endif