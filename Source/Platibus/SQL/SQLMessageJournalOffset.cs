using System;
using Platibus.Journaling;

namespace Platibus.SQL
{
    internal class SQLMessageJournalOffset : MessageJournalOffset, IEquatable<SQLMessageJournalOffset>
    {
        private readonly long _id;

        public long Id { get { return _id; } }

        public SQLMessageJournalOffset(long id)
        {
            _id = id;
        }

        public override string ToString()
        {
            return _id.ToString("D");
        }

        public bool Equals(SQLMessageJournalOffset other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _id == other._id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((SQLMessageJournalOffset) obj);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public static bool operator ==(SQLMessageJournalOffset left, SQLMessageJournalOffset right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SQLMessageJournalOffset left, SQLMessageJournalOffset right)
        {
            return !Equals(left, right);
        }
    }
}
