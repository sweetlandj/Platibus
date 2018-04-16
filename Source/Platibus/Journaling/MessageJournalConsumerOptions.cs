// The MIT License (MIT)
// 
// Copyright (c) 2018 Jesse Sweetland
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
    /// Options to customize the behavior of the <see cref="MessageJournalConsumer"/>
    /// </summary>
    public class MessageJournalConsumerOptions
    {
        /// <summary>
        /// The default <see cref="PollingInterval"/>
        /// </summary>
        public static readonly TimeSpan DefaultPollingInterval = TimeSpan.FromSeconds(3);

        /// <summary>
        /// The default <see cref="BatchSize"/>
        /// </summary>
        public const int DefaultBatchSize = 100;

        /// <summary>
        /// An optional filter to apply when reading journal entries from the journal
        /// </summary>
        public MessageJournalFilter Filter { get; set; }

        /// <summary>
        /// The number of journaled events to request in each read operation
        /// </summary>
        public int BatchSize { get; set; } = 100;

        /// <summary>
        /// Whether to stop consuming events when an exception is thrown from a message
        /// handler or a message is not acknowledged by at least one handler.
        /// </summary>
        public bool RethrowExceptions { get; set; }

        /// <summary>
        /// Whether to stop processing when the end of the journal is reached.
        /// </summary>
        /// <remarks>
        /// If <c>false</c>, the consumer will wait for the specified <see cref="PollingInterval"/>
        /// before attempting to fetch another <see cref="BatchSize"/> entries from the journal.
        /// </remarks>
        public bool HaltAtEndOfJournal { get; set; }

        /// <summary>
        /// The length of time to pause before attempting to read new events once the
        /// end of the journal has been reached.
        /// </summary>
        /// <remarks>
        /// The polling interval is only used when <see cref="HaltAtEndOfJournal"/> is <c>false</c>.
        /// </remarks>
        public TimeSpan PollingInterval { get; set; } = DefaultPollingInterval;

        /// <summary>
        /// An optional callback for reporting message consumer progress
        /// </summary>
        public IProgress<MessageJournalConsumerProgress> Progress { get; set; }
    }
}
