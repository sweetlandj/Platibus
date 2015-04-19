using System;

namespace Platibus.SQL
{
    public class SQLSubscription : IEquatable<SQLSubscription>
    {
        private readonly TopicName _topicName;
        private readonly Uri _subscriber;
        private readonly DateTime _expires;

        public SQLSubscription(TopicName topicName, Uri subscriber, DateTime expires)
        {
            if (topicName == null) throw new ArgumentNullException("topicName");
            if (subscriber == null) throw new ArgumentNullException("subscriber");
            _topicName = topicName;
            _subscriber = subscriber;
            _expires = expires;
        }

        public TopicName TopicName
        {
            get { return _topicName; }
        }

        public Uri Subscriber
        {
            get { return _subscriber; }
        }

        public DateTime Expires
        {
            get { return _expires; }
        }

        public bool Equals(SQLSubscription subscription)
        {
            if (ReferenceEquals(this, subscription)) return true;
            if (ReferenceEquals(null, subscription)) return false;
            return Equals(_topicName, subscription._topicName) && Equals(_subscriber, subscription._subscriber);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SQLSubscription);
        }

        public override int GetHashCode()
        {
            return _subscriber.GetHashCode();
        }
    }
}
