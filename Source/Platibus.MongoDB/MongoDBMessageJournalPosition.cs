using System;
using MongoDB.Bson;
using Platibus.Journaling;

namespace Platibus.MongoDB
{
    internal class MongoDBMessageJournalPosition : MessageJournalPosition
    {
        private readonly BsonTimestamp _timestamp;

        public BsonTimestamp Timestamp
        {
            get { return _timestamp; }
        }

        public MongoDBMessageJournalPosition(BsonTimestamp timestamp)
        {
            if (timestamp == null) throw new ArgumentNullException("timestamp");
            _timestamp = timestamp;
        }

        public override string ToString()
        {
            return _timestamp.Value.ToString("D");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(obj, null)) return false;
            var other = obj as MongoDBMessageJournalPosition;
            return other != null && _timestamp.Equals(other._timestamp);
        }

        public override int GetHashCode()
        {
            return _timestamp.GetHashCode();
        }

        public static MongoDBMessageJournalPosition Parse(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;
            var value = long.Parse(str);
            var ts = new BsonTimestamp(value);
            return new MongoDBMessageJournalPosition(ts);
        }
    }
}
