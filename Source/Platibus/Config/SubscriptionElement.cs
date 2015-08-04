// The MIT License (MIT)
// 
// Copyright (c) 2015 Jesse Sweetland
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

namespace Platibus.Config
{
    /// <summary>
    /// Configuration element for subscriptions
    /// </summary>
    public class SubscriptionElement : ConfigurationElement
    {
        private const string EndpointPropertyName = "endpoint";
        private const string TopicPropertyName = "topic";
        private const string TTLPropertyName = "ttl";

        /// <summary>
        /// The name of the endpoint in which the topic is hosted
        /// </summary>
        [ConfigurationProperty(EndpointPropertyName, IsRequired = true, IsKey = true)]
        public string Endpoint
        {
            get { return (string) base[EndpointPropertyName]; }
            set { base[EndpointPropertyName] = value; }
        }

        /// <summary>
        /// The name of the topic
        /// </summary>
        [ConfigurationProperty(TopicPropertyName, IsRequired = true, IsKey = true)]
        public string Topic
        {
            get { return (string) base[TopicPropertyName]; }
            set { base[TopicPropertyName] = value; }
        }

        /// <summary>
        /// The Time-To-Live (TTL) for the subscription
        /// </summary>
        /// <remarks>
        /// Subscriptions will regularly be renewed, but the TTL serves as a
        /// "dead man's switch" that will cause the subscription to be terminated
        /// if not renewed within that span of time.
        /// </remarks>
        [ConfigurationProperty(TTLPropertyName, DefaultValue = "24:00:00")]
        public TimeSpan TTL
        {
            get { return (TimeSpan) base[TTLPropertyName]; }
            set { base[TTLPropertyName] = value; }
        }
    }
}