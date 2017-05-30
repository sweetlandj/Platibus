using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Journaling;

namespace Platibus.UnitTests.Http
{
    public class MessageJournalStub : IMessageJournal
    {
        private readonly object _syncRoot = new object();
        private readonly IList<MessageJournalEntry> _entries = new List<MessageJournalEntry>();

        public virtual Task<MessageJournalPosition> GetBeginningOfJournal(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult<MessageJournalPosition>(new Position(0));
        }

        public virtual Task Append(Message message, MessageJournalCategory category,
            CancellationToken cancellationToken = new CancellationToken())
        {
            lock (_syncRoot)
            {
                var position = new Position(_entries.Count);
                _entries.Add(new MessageJournalEntry(category, position, DateTime.UtcNow, message));
            }
            return Task.FromResult(0);
        }

        public virtual Task<MessageJournalReadResult> Read(MessageJournalPosition start, int count, MessageJournalFilter filter = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var skip = ((Position) start).Index;
            IList<MessageJournalEntry> data;
            lock (_syncRoot)
            {
                var filteredEntries = _entries
                    .Skip(skip);

                if (filter != null)
                {
                    if (filter.Topics.Any())
                    {
                        filteredEntries = filteredEntries
                            .Where(e => filter.Topics.Contains(e.Data.Headers.Topic));
                    }

                    if (filter.Categories.Any())
                    {
                        filteredEntries = filteredEntries
                            .Where(e => filter.Categories.Contains(e.Category));
                    }
                }

                data = filteredEntries.Take(count + 1).ToList();
            }

            var endOfJournal = data.Count <= count;

            MessageJournalPosition next;
            if (!data.Any())
            {
                next = start;
            }
            else if (endOfJournal)
            {
                var lastIndex = data.Select(e => ((Position)e.Position).Index).LastOrDefault();
                next = new Position(lastIndex + 1);
            }
            else
            {
                next = data.Select(e => e.Position).LastOrDefault();
            }

            var result = new MessageJournalReadResult(start, next, endOfJournal, data.Take(count));
            return Task.FromResult(result);
        }

        public virtual MessageJournalPosition ParsePosition(string str)
        {
            return new Position(int.Parse(str));
        }

        internal class Position : MessageJournalPosition, IEquatable<Position>
        {
            private readonly int _index;

            public int Index { get { return _index; } }
             
            public Position(int index)
            {
                _index = index;
            }

            public override string ToString()
            {
                return _index.ToString("D");
            }

            public bool Equals(Position other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return _index == other._index;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((Position) obj);
            }

            public override int GetHashCode()
            {
                return _index;
            }

            public static bool operator ==(Position left, Position right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(Position left, Position right)
            {
                return !Equals(left, right);
            }
        }
    }
}
