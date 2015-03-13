// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
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

namespace Platibus
{
    public struct QueueOptions
    {
        public const int DefaultConcurrencyLimit = 16;

        /// <summary>
        ///     The maximum number of messages that will be processed concurrently.
        /// </summary>
        /// <remarks>
        ///     If this value is less than or equal to <c>0</c> the <see cref="DefaultConcurrencyLimit" />
        ///     will be applied.
        /// </remarks>
        /// <seealso cref="DefaultConcurrencyLimit" />
        public int ConcurrencyLimit { get; set; }

        /// <summary>
        ///     Whether the messages should be acknowledged automatically.  If auto-acknowledge
        ///     is set to <code>false</code> the <see cref="IQueueListener" /> must explicitly
        ///     acknowledge each message.  Otherwise the message will be acknowledged automatically
        ///     provided an exception is not thrown from the <see cref="IQueueListener.MessageReceived" />
        ///     method.
        /// </summary>
        public bool AutoAcknowledge { get; set; }

        /// <summary>
        ///     The maximum number of attempts to process the message.  If the <see cref="IQueueListener" />
        ///     does not acknowledge the message, or the queue is set to <see cref="AutoAcknowledge" /> and
        ///     an exception is thrown from the <see cref="IQueueListener.MessageReceived" /> method, the
        ///     message will be put back on the queue and retried up to this maximum number of attempts.
        /// </summary>
        public int MaxAttempts { get; set; }

        /// <summary>
        ///     The amount of time to wait before putting an unacknowledged message back on the queue.
        /// </summary>
        public TimeSpan RetryDelay { get; set; }
    }
}