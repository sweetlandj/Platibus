using System;

namespace Platibus.UnitTests.Stubs
{
    public class SubscriptionAddedEventArgs : EventArgs
    {
        private readonly TopicName _topic;
        private readonly Uri _subscriber;
        private readonly TimeSpan _ttl;

        public TopicName Topic { get { return _topic; } }
        public Uri Subscriber { get { return _subscriber; } }
        public TimeSpan TTL { get { return _ttl; } }

        public SubscriptionAddedEventArgs(TopicName topic, Uri subscriber, TimeSpan ttl)
        {
            _topic = topic;
            _subscriber = subscriber;
            _ttl = ttl;
        }
    }
}
