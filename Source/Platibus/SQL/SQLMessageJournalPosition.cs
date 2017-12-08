using System;
using Platibus.Journaling;

namespace Platibus.SQL
{
    internal class SQLMessageJournalPosition : MessageJournalPosition, IEquatable<SQLMessageJournalPosition>
    {
        public long Id { get; }

        public SQLMessageJournalPosition(long id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return Id.ToString("D");
        }

        public bool Equals(SQLMessageJournalPosition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((SQLMessageJournalPosition) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(SQLMessageJournalPosition left, SQLMessageJournalPosition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SQLMessageJournalPosition left, SQLMessageJournalPosition right)
        {
            return !Equals(left, right);
        }
    }
}
