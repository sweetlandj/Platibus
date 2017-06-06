using System;
using MongoDB.Bson;
using Platibus.Journaling;

namespace Platibus.MongoDB
{
    internal class MongoDBMessageJournalPosition : MessageJournalPosition
    {
        private readonly ObjectId _id;

        public ObjectId Id
        {
            get { return _id; }
        }

        public MongoDBMessageJournalPosition(ObjectId id)
        {
            if (id == null) throw new ArgumentNullException("id");
            _id = id;
        }

        public override string ToString()
        {
            return _id.ToString();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(obj, null)) return false;
            var other = obj as MongoDBMessageJournalPosition;
            return other != null && _id.Equals(other._id);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public static MongoDBMessageJournalPosition Parse(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;
            var id = ObjectId.Parse(str);
            return new MongoDBMessageJournalPosition(id);
        }
    }
}
