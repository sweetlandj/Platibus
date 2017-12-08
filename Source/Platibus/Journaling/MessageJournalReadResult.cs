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

namespace Platibus.Journaling
{
    /// <summary>
    /// The result of a journal read
    /// </summary>
    public class MessageJournalReadResult
    {
        private readonly IList<MessageJournalEntry> _entries;

        /// <summary>
        /// The position at which the read started
        /// </summary>
        public MessageJournalPosition Start { get; }

        /// <summary>
        /// The next position to read from
        /// </summary>
        public MessageJournalPosition Next { get; }

        /// <summary>
        /// Whether the end of the journal was reached during the read operation
        /// </summary>
        public bool EndOfJournal { get; }

        /// <summary>
        /// The journaled messages that were read
        /// </summary>
        public IEnumerable<MessageJournalEntry> Entries => _entries;

        /// <summary>
        /// Initializes a new <see cref="MessageJournalReadResult"/>
        /// </summary>
        /// <param name="start">The position at which the read started</param>
        /// <param name="next">The next position to read from</param>
        /// <param name="endOfJournal">Whether the end of the journal was reached during the read operation</param>
        /// <param name="messages">The journaled messages that were read</param>
        public MessageJournalReadResult(MessageJournalPosition start, MessageJournalPosition next, bool endOfJournal, IEnumerable<MessageJournalEntry> messages)
        {
            Start = start ?? throw new ArgumentNullException(nameof(start));
            Next = next ?? throw new ArgumentNullException(nameof(next));
            EndOfJournal = endOfJournal;
            _entries = (messages ?? Enumerable.Empty<MessageJournalEntry>()).ToList();
        }
    }
}
