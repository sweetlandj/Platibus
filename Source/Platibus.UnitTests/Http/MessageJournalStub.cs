// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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
            public int Index { get; }

            public Position(int index)
            {
                Index = index;
            }

            public override string ToString()
            {
                return Index.ToString("D");
            }

            public bool Equals(Position other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Index == other.Index;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((Position) obj);
            }

            public override int GetHashCode()
            {
                return Index;
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
