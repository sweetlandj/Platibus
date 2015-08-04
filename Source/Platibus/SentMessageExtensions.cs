// The MIT License (MIT)
// 
// Copyright (c) 2015 Jesse Sweetland
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
using System.Threading;
using System.Threading.Tasks;

namespace Platibus
{
    /// <summary>
    /// Extension methods for working with <see cref="ISentMessage"/>s
    /// </summary>
    public static class SentMessageExtensions
    {
        /// <summary>
        /// Observes the sent message replies until the first reply is received
        /// or a timeout occurs
        /// </summary>
        /// <param name="sentMessage">The sent message</param>
        /// <param name="timeout">The maximum amount of time to wait for a reply</param>
        /// <returns>Returns a task whose result will be the deserialized content of the 
        /// reply message</returns>
        public static async Task<object> GetReply(this ISentMessage sentMessage, TimeSpan timeout = default(TimeSpan))
        {
            object reply = null;
            using (var replyReceivedEvent = new ManualResetEvent(false))
            using (sentMessage.ObserveReplies().Subscribe(r =>
            {
                reply = r;
                // ReSharper disable once AccessToDisposedClosure
                replyReceivedEvent.Set();
            }))
            {
                await replyReceivedEvent.WaitOneAsync(timeout);    
            }
            return reply;
        }
    }
}