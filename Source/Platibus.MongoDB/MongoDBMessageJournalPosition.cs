using MongoDB.Bson;
using Platibus.Journaling;

namespace Platibus.MongoDB
{
    internal class MongoDBMessageJournalPosition : MessageJournalPosition
    {
        public ObjectId Id { get; }

        public MongoDBMessageJournalPosition(ObjectId id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return Id.ToString();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is MongoDBMessageJournalPosition other && Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static MongoDBMessageJournalPosition Parse(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;
            var id = ObjectId.Parse(str);
            return new MongoDBMessageJournalPosition(id);
        }
    }
}
