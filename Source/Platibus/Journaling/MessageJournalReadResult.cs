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
        private readonly MessageJournalOffset _start;
        private readonly MessageJournalOffset _next;
        private readonly bool _endOfJournal;
        private readonly IList<JournaledMessage> _journaledMessages;

        /// <summary>
        /// The position at which the read started
        /// </summary>
        public MessageJournalOffset Start { get { return _start; } }

        /// <summary>
        /// The next position to read from
        /// </summary>
        public MessageJournalOffset Next { get { return _next; } }

        /// <summary>
        /// Whether the end of the journal was reached during the read operation
        /// </summary>
        public bool EndOfJournal { get { return _endOfJournal; } }

        /// <summary>
        /// The journaled messages that were read
        /// </summary>
        public IEnumerable<JournaledMessage> JournaledMessages { get { return _journaledMessages; } }

        /// <summary>
        /// Initializes a new <see cref="MessageJournalReadResult"/>
        /// </summary>
        /// <param name="start">The position at which the read started</param>
        /// <param name="next">The next position to read from</param>
        /// <param name="endOfJournal">Whether the end of the journal was reached during the read operation</param>
        /// <param name="messages">The journaled messages that were read</param>
        public MessageJournalReadResult(MessageJournalOffset start, MessageJournalOffset next, bool endOfJournal, IEnumerable<JournaledMessage> messages)
        {
            if (start == null) throw new ArgumentNullException("start");
            if (next == null) throw new ArgumentNullException("next");
            _start = start;
            _next = next;
            _endOfJournal = endOfJournal;
            _journaledMessages = (messages ?? Enumerable.Empty<JournaledMessage>()).ToList();
        }
    }
}
