// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
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
using System.Collections.Generic;
using Platibus.Serialization;

namespace Platibus.Config
{
    /// <summary>
    ///     Concrete mutable implementation of <see cref="IPlatibusConfiguration" /> used for
    ///     programmatically configuring the bus.
    /// </summary>
    /// <remarks>
    ///     This class is not threadsafe.  The caller must provide synchronization if there is
    ///     a possibility for multiple threads to make conccurrent updates to instances of this
    ///     class.
    /// </remarks>
    public class PlatibusConfiguration : IPlatibusConfiguration
    {
        private Uri _baseUri;
        private readonly IDictionary<EndpointName, IEndpoint> _endpoints = new Dictionary<EndpointName, IEndpoint>();
        private readonly IList<IHandlingRule> _handlingRules = new List<IHandlingRule>();
        private readonly IList<ISendRule> _sendRules = new List<ISendRule>();
        private readonly IList<ISubscription> _subscriptions = new List<ISubscription>();
        private readonly IList<TopicName> _topics = new List<TopicName>();

        public Uri BaseUri
        {
            get { return _baseUri ?? (_baseUri = new Uri("http://localhost/platibus")); }
            set { _baseUri = value; }
        }

        public IMessageNamingService MessageNamingService { get; set; }
        public ISerializationService SerializationService { get; set; }
        public IMessageJournalingService MessageJournalingService { get; set; }
        public IMessageQueueingService MessageQueueingService { get; set; }
        public ISubscriptionTrackingService SubscriptionTrackingService { get; set; }

        public IEnumerable<KeyValuePair<EndpointName, IEndpoint>> Endpoints
        {
            get { return _endpoints; }
        }

        public IEnumerable<TopicName> Topics
        {
            get { return _topics; }
        }

        public IEnumerable<ISendRule> SendRules
        {
            get { return _sendRules; }
        }

        public IEnumerable<IHandlingRule> HandlingRules
        {
            get { return _handlingRules; }
        }

        public IEnumerable<ISubscription> Subscriptions
        {
            get { return _subscriptions; }
        }

        public void AddEndpoint(EndpointName name, IEndpoint endpoint)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (endpoint == null) throw new ArgumentNullException("endpoint");
            if (_endpoints.ContainsKey(name)) throw new EndpointAlreadyExistsException(name);
            _endpoints[name] = endpoint;
        }

        public void AddTopic(TopicName topic)
        {
            if (topic == null) throw new ArgumentNullException("topic");
            if (_topics.Contains(topic)) throw new TopicAlreadyExistsException(topic);
            _topics.Add(topic);
        }

        public void AddSendRule(ISendRule sendRule)
        {
            if (sendRule == null) throw new ArgumentNullException("sendRule");
            _sendRules.Add(sendRule);
        }

        public void AddHandlingRule(IMessageSpecification specification, IMessageHandler messageHandler, QueueName queueName = null)
        {
            if (specification == null) throw new ArgumentNullException("specification");
            if (messageHandler == null) throw new ArgumentNullException("messageHandler");
            _handlingRules.Add(new HandlingRule(specification, messageHandler, queueName));
        }

        public void AddSubscription(ISubscription subscription)
        {
            if (subscription == null) throw new ArgumentNullException("subscription");
            _subscriptions.Add(subscription);
        }

        public IEndpoint GetEndpoint(EndpointName endpointName)
        {
            if (endpointName == null) throw new ArgumentNullException("endpointName");
            IEndpoint endpoint;
            if (!_endpoints.TryGetValue(endpointName, out endpoint) || endpoint == null)
            {
                throw new EndpointNotFoundException(endpointName);
            }
            return endpoint;
        }
    }
}