using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Platibus.InMemory;

namespace Platibus.UnitTests.Stubs
{
    public class SubscriptionTrackingServiceStub : ISubscriptionTrackingService
    {
        private readonly InMemorySubscriptionTrackingService _storage = new InMemorySubscriptionTrackingService();

        public event SubscriptionAddedEventHandler SubscriptionAdded;
        public event SubscriptionRemovedEventHandler SubscriptionRemoved;

        public Task AddSubscription(TopicName topic, Uri subscriber, TimeSpan ttl = new TimeSpan(),
            CancellationToken cancellationToken = new CancellationToken())
        {
            var result = _storage.AddSubscription(topic, subscriber, ttl, cancellationToken);
            var handlers = SubscriptionAdded;
            if (handlers != null)
            {
                handlers(this, new SubscriptionAddedEventArgs(topic, subscriber, ttl));
            }
            return result;
        }

        public Task RemoveSubscription(TopicName topic, Uri subscriber, CancellationToken cancellationToken = new CancellationToken())
        {
            var result = _storage.RemoveSubscription(topic, subscriber, cancellationToken);
            var handlers = SubscriptionRemoved;
            if (handlers != null)
            {
                handlers(this, new SubscriptionRemovedEventArgs(topic, subscriber));
            }
            return result;
        }

        public Task<IEnumerable<Uri>> GetSubscribers(TopicName topic, CancellationToken cancellationToken = new CancellationToken())
        {
            return _storage.GetSubscribers(topic, cancellationToken);
        }
    }
}
