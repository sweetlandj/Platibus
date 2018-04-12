using System;
using Platibus.Journaling;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.UnitTests.Journaling
{
    public class MessageJournalStub : IMessageJournal
    {
        private readonly object _syncRoot = new object();

        private readonly IList<MessageJournalEntry> _entries = new List<MessageJournalEntry>();

        public virtual Task<MessageJournalPosition> GetBeginningOfJournal(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult<MessageJournalPosition>(new Position(0));
        }

        public virtual Task Append(Message message, MessageJournalCategory category,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (_syncRoot)
            {
                var position = new Position(_entries.Count);
                _entries.Add(new MessageJournalEntry(category, position, DateTime.UtcNow, message));
            }

            return Task.FromResult(0);
        }

        public virtual Task<MessageJournalReadResult> Read(MessageJournalPosition start, int count, MessageJournalFilter filter = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var myStart = (Position)start;
            IList<MessageJournalEntry> myEntries;
            lock (_syncRoot)
            {
                myEntries = _entries.Skip(myStart.Index)
                    .Where(entry => MatchesFilter(filter, entry))
                    .Take(count + 1)
                    .ToList();
            }

            var next = myStart;
            var endOfJournal = myEntries.Count <= count;
            if (myEntries.Any())
            {
                var lastIndex = myEntries.Select(e => e.Position).OfType<Position>().Max(p => p.Index);
                next = new Position(lastIndex + 1);
            }
            var readResult = new MessageJournalReadResult(start, next, endOfJournal, myEntries.Take(count));
            return Task.FromResult(readResult);
        }

        private static bool MatchesFilter(MessageJournalFilter filter, MessageJournalEntry entry)
        {
            if (filter == null) return true;

            var headers = entry.Data.Headers;
            
            if (!string.IsNullOrWhiteSpace(filter.MessageName))
            {
                var messageName = (string)headers.MessageName;
                if (!messageName.Contains(filter.MessageName)) return false;
            }

            if (filter.Topics.Any())
            {
                var topic = headers.Topic;
                if (!filter.Topics.Contains(topic)) return false;
            }

            if (filter.Categories.Any())
            {
                var category = entry.Category;
                if (!filter.Categories.Contains(category)) return false;
            }

            var timestamp = entry.Timestamp;
            if (filter.From > timestamp) return false;
            if (filter.To <= timestamp) return false;

            if (filter.Origination != null)
            {
                var origination = headers.Origination;
                if (!origination.Equals(filter.Origination)) return false;
            }

            if (filter.Destination != null)
            {
                var destination = headers.Destination;
                if (!destination.Equals(filter.Destination)) return false;
            }

            if (filter.RelatedTo != null)
            {
                var relatedTo = headers.RelatedTo;
                if (!relatedTo.Equals(filter.RelatedTo)) return false;
            }

            return true;
        }

        public virtual MessageJournalPosition ParsePosition(string str)
        {
            return new Position(int.Parse(str));
        }

        public class Position : MessageJournalPosition
        {
            public int Index { get; }

            public Position(int index)
            {
                Index = index;
            }

            public override string ToString()
            {
                return Index.ToString("D");
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as Position);
            }

            public bool Equals(Position p)
            {
                return Equals(Index, p?.Index);
            }

            public override int GetHashCode()
            {
                return Index;
            }
        }
    }
}
