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

namespace Platibus.Journaling
{
    /// <summary>
    /// A message stored in a <see cref="IMessageJournal"/>
    /// </summary>
    public class MessageJournalEntry
    {
        /// <summary>
        /// The category of journaled message (e.g. sent, received, published)
        /// </summary>
        public MessageJournalCategory Category { get; }

        /// <summary>
        /// The position of the message in the journal
        /// </summary>
        public MessageJournalPosition Position { get; }

        /// <summary>
        /// The timestamp for the journaled message
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// The message
        /// </summary>
        public Message Data { get; }

        /// <summary>
        /// Initializes a new <see cref="MessageJournalEntry"/>
        /// </summary>
        /// <param name="category">The category of journaled message (e.g. sent, received, published)</param>
        /// <param name="position">The position of the message in the journal</param>
        /// <param name="timestamp">The timestamp associated with the journal entry</param>
        /// <param name="data">The message</param>
        public MessageJournalEntry(MessageJournalCategory category, MessageJournalPosition position, DateTime timestamp, Message data)
        {
            Category = category;
            Position = position ?? throw new ArgumentNullException(nameof(position));
            Timestamp = timestamp;
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }
    }
}
