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
    public class JournaledMessage
    {
        private readonly JournaledMessageCategory _category;
        private readonly MessageJournalOffset _offset;
        private readonly Message _message;

        /// <summary>
        /// The category of journaled message (e.g. sent, received, published)
        /// </summary>
        public JournaledMessageCategory Category { get { return _category; } }
        
        /// <summary>
        /// The position of the message in the journal
        /// </summary>
        public MessageJournalOffset Offset { get { return _offset; } }

        /// <summary>
        /// The message
        /// </summary>
        public Message Message { get { return _message; } }

        /// <summary>
        /// Initializes a new <see cref="JournaledMessage"/>
        /// </summary>
        /// <param name="category">The category of journaled message (e.g. sent, received, published)</param>
        /// <param name="offset">The position of the message in the journal</param>
        /// <param name="message">The message</param>
        public JournaledMessage(JournaledMessageCategory category, MessageJournalOffset offset, Message message)
        {
            if (offset == null) throw new ArgumentNullException("offset");
            if (message == null) throw new ArgumentNullException("message");
            _category = category;
            _offset = offset;
            _message = message;
        }
    }
}
