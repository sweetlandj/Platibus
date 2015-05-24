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

using System.Configuration;

namespace Platibus.Config
{
    public class PlatibusConfigurationSection : ConfigurationSection
    {
        
        private const string JournalingPropertyName = "journaling";
        private const string QueueingPropertyName = "queueing";
        
        private const string TimeoutsPropertyName = "timeouts";
        private const string EndpointsPropertyName = "endpoints";
        private const string TopicsPropertyName = "topics";
        private const string SendRulesPropertyName = "sendRules";
        private const string SubscriptionsPropertyName = "subscriptions";

        public PlatibusConfigurationSection()
        {
            Timeouts = new TimeoutsElement();
            Endpoints = new EndpointElementCollection();
            Topics = new TopicElementCollection();
            SendRules = new SendRuleElementCollection();
        }
[ConfigurationProperty(JournalingPropertyName, IsRequired=false, DefaultValue=null)]
        public JournalingElement Journaling
        {
            get { return (JournalingElement)base[JournalingPropertyName]; }
            set { base[JournalingPropertyName] = value; }
        }

        [ConfigurationProperty(QueueingPropertyName)]
        public QueueingElement Queueing
        {
            get { return (QueueingElement) base[QueueingPropertyName]; }
            set { base[QueueingPropertyName] = value; }
        }

        [ConfigurationProperty(TimeoutsPropertyName)]
        public TimeoutsElement Timeouts
        {
            get { return (TimeoutsElement) base[TimeoutsPropertyName]; }
            set { base[TimeoutsPropertyName] = value; }
        }

        [ConfigurationProperty(EndpointsPropertyName)]
        [ConfigurationCollection(typeof (EndpointElement),
            CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
        public EndpointElementCollection Endpoints
        {
            get { return (EndpointElementCollection) base[EndpointsPropertyName]; }
            set { base[EndpointsPropertyName] = value; }
        }

        [ConfigurationProperty(TopicsPropertyName)]
        [ConfigurationCollection(typeof (TopicElement),
            CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
        public TopicElementCollection Topics
        {
            get { return (TopicElementCollection) base[TopicsPropertyName]; }
            set { base[TopicsPropertyName] = value; }
        }

        [ConfigurationProperty(SendRulesPropertyName)]
        [ConfigurationCollection(typeof (EndpointElement),
            CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
        public SendRuleElementCollection SendRules
        {
            get { return (SendRuleElementCollection) base[SendRulesPropertyName]; }
            set { base[SendRulesPropertyName] = value; }
        }

        [ConfigurationProperty(SubscriptionsPropertyName)]
        [ConfigurationCollection(typeof (EndpointElement),
            CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
        public SubscriptionElementCollection Subscriptions
        {
            get { return (SubscriptionElementCollection) base[SubscriptionsPropertyName]; }
            set { base[SubscriptionsPropertyName] = value; }
        }
    }
}