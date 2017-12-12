// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
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
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Platibus.UnitTests
{
    [Trait("Category", "UnitTests")]
    public abstract class SubscriptionTrackingServiceTests<TSubscriptionTrackingService>
        where TSubscriptionTrackingService : ISubscriptionTrackingService
    {
        protected readonly TSubscriptionTrackingService SubscriptionTrackingService;

        protected SubscriptionTrackingServiceTests(TSubscriptionTrackingService subscriptionTrackingService)
        {
            SubscriptionTrackingService = subscriptionTrackingService;
        }

        [Fact]
        public async Task AddingSubscriptionAddsSubscriberToTopic()
        {
            var topic = GenerateUniqueTopicName();
            var subscriberUri = new Uri("http://localhost/platibus/");
            await WhenSubscriptionIsAdded(topic, subscriberUri);
            await AssertSubscriberAddedToTopic(topic, subscriberUri);
        }

        [Fact]
        public async Task RemovingSubscriptionRemovesSubscriberFromTopic()
        {
            var topic = GenerateUniqueTopicName();
            var subscriberUri = new Uri("http://localhost/platibus/");
            await GivenASubscription(topic, subscriberUri);
            await WhenTheSubscriptionIsRemoved(topic, subscriberUri);
            await AssertSubscriberRemovedFromTopic(topic, subscriberUri);
        }

        [Fact]
        public async Task SubscriptionExpirationRemovesSubscriberFromTopic()
        {
            var topic = GenerateUniqueTopicName();
            var subscriberUri = new Uri("http://localhost/platibus/");
            var ttl = TimeSpan.FromSeconds(1);
            await GivenASubscription(topic, subscriberUri, ttl);
            var untilExpired = ttl.Add(TimeSpan.FromMilliseconds(100));
            await Task.Delay(untilExpired);
            await AssertSubscriberRemovedFromTopic(topic, subscriberUri);
        }

        [Fact]
        public async Task HandlesManyConcurrentOperationsWithoutError()
        {
            var subscriptions = GivenManySubscriptions().ToList();
            await WhenAddingSubscriptionsConcurrently(subscriptions);
            await AssertSubscribersAddedToTopics(subscriptions);
        }

        protected TopicName GenerateUniqueTopicName()
        {
            return Guid.NewGuid().ToString("N");
        }

        protected Task GivenASubscription(TopicName topic, Uri subscriber, TimeSpan ttl = default(TimeSpan))
        {
            return SubscriptionTrackingService.AddSubscription(topic, subscriber, ttl);
        }

        protected IEnumerable<Subscription> GivenManySubscriptions(int topicCount = 10, int subscribersPerTopic = 100)
        {
            var topicNames = Enumerable.Range(0, topicCount)
                .Select(i => new TopicName("Topic-" + i));

            return topicNames
                .SelectMany(t => Enumerable.Range(0, subscribersPerTopic)
                    .Select(j => new Uri("http://localhost/subscriber-" + j + "/"))
                    .Select(s => new Subscription(t, s)));
        }

        protected Task WhenSubscriptionIsAdded(TopicName topic, Uri subscriber, TimeSpan ttl = default(TimeSpan))
        {
            return SubscriptionTrackingService.AddSubscription(topic, subscriber, ttl);
        }

        protected Task WhenTheSubscriptionIsRemoved(TopicName topic, Uri subscriber)
        {
            return SubscriptionTrackingService.RemoveSubscription(topic, subscriber);
        }

        protected Task WhenAddingSubscriptionsConcurrently(IEnumerable<Subscription> subscriptions)
        {
            var tasks = subscriptions
                .Select(s => SubscriptionTrackingService.AddSubscription(s.Topic, s.SubscriberUri));

            return Task.WhenAll(tasks);
        }

        protected async Task AssertSubscribersAddedToTopics(IEnumerable<Subscription> subscriptions)
        {
            var subscriptionsByTopic = subscriptions.GroupBy(s => s.Topic);
            foreach (var grouping in subscriptionsByTopic)
            {
                var expectedSubscribers = grouping.Select(g => g.SubscriberUri)
                    .OrderBy(uri => uri.ToString())
                    .ToList();
                var actualSubscribers = (await SubscriptionTrackingService
                    .GetSubscribers(grouping.Key))
                    .OrderBy(uri => uri.ToString())
                    .ToList();
                
                Assert.Equal(expectedSubscribers, actualSubscribers);
            }
        }

        protected async Task AssertSubscriberAddedToTopic(TopicName topic, Uri subscriberUri)
        {
            var subscribers = await SubscriptionTrackingService.GetSubscribers(topic);
            Assert.Contains(subscriberUri, subscribers);
        }

        protected async Task AssertSubscriberRemovedFromTopic(TopicName topic, Uri subscriberUri)
        {
            var subscribers = await SubscriptionTrackingService.GetSubscribers(topic);
            Assert.DoesNotContain(subscriberUri, subscribers);
        }

        protected class Subscription
        {
            public TopicName Topic { get; }
            public Uri SubscriberUri { get; }

            public Subscription(TopicName topic, Uri subscriberUri)
            {
                Topic = topic;
                SubscriberUri = subscriberUri;
            }
        }
    }
}
