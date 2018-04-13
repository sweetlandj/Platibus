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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.UnitTests
{
    public class MessageHandlerStub : IMessageHandler
    {
        private static readonly AutoResetEvent MessageReceivedEvent = new AutoResetEvent(false);
        private static readonly ConcurrentQueue<object> HandledMessageQueue = new ConcurrentQueue<object>();

        public WaitHandle WaitHandle
        {
            get { return MessageReceivedEvent; }
        }

        public IEnumerable<object> HandledMessages
        {
            get { return HandledMessageQueue; }
        }

        public string Name
        {
            get { return "TestMessageHandler"; }
        }

        public Task HandleMessage(object content, IMessageContext messageContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            HandledMessageQueue.Enqueue(content);
            messageContext.Acknowledge();
            MessageReceivedEvent.Set();
            return Task.FromResult(true);
        }

        public static void Reset()
        {
            while (HandledMessageQueue.TryDequeue(out _))
            {
            }
        }
    }
}