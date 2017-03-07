using System;

namespace Platibus.UnitTests.Stubs
{
    public class SubscriptionRemovedEventArgs : EventArgs
    {
        private readonly TopicName _topic;
        private readonly Uri _subscriber;

        public TopicName Topic { get { return _topic; } }
        public Uri Subscriber { get { return _subscriber; } }

        public SubscriptionRemovedEventArgs(TopicName topic, Uri subscriber)
        {
            _topic = topic;
            _subscriber = subscriber;
        }
    }
}