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

namespace Platibus.IIS
{
    /// <summary>
    /// Configuration section for IIS HTTP module and handler
    /// </summary>
    public class IISConfigurationSection : PlatibusConfigurationSection
    {
        private const string BaseUriPropertyName = "baseUri";
        private const string SubscriptionTrackingPropertyName = "subscriptionTracking";
        private const string QueueingPropertyName = "queueing";
        private const string BypassTransportLocalDestinationPropertyName = "bypassTransportLocalDestination";

        /// <summary>
        /// The base URI of this instance
        /// </summary>
        /// <remarks>
        /// Must agree with at least one of the bindings for the hosting web application
        /// </remarks>
        [ConfigurationProperty(BaseUriPropertyName)]
        public Uri BaseUri
        {
            get => (Uri) base[BaseUriPropertyName];
            set => base[BaseUriPropertyName] = value;
        }

        /// <summary>
        /// The subscription tracking configuration
        /// </summary>
        [ConfigurationProperty(SubscriptionTrackingPropertyName)]
        public SubscriptionTrackingElement SubscriptionTracking
        {
            get => (SubscriptionTrackingElement) base[SubscriptionTrackingPropertyName];
            set => base[SubscriptionTrackingPropertyName] = value;
        }

        /// <summary>
        /// The queueing configuration
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