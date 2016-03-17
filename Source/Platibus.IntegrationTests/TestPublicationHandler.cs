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

using System.Threading;
using System.Threading.Tasks;

namespace Platibus.IntegrationTests
{
    public class TestPublicationHandler : IMessageHandler
    {
        private static readonly object SyncRoot = new object();
        private volatile static CountdownEvent _messageReceivedEvent = new CountdownEvent(1);

        public static CountdownEvent MessageReceivedEvent { get { return _messageReceivedEvent; } }

        public static WaitHandle WaitHandle
        {
            get
            {
                lock (SyncRoot)
                {
                    return _messageReceivedEvent.WaitHandle;
                }
            }
        }

        public string Name
        {
            get { return "PublicationHandler"; }
        }

        public Task HandleMessage(object content, IMessageContext messageContext, CancellationToken cancellationToken)
        {
            messageContext.Acknowledge();
            lock (SyncRoot)
            {
                _messageReceivedEvent.Signal();
            }
            return Task.FromResult(true);
        }

        public static void Reset()
        {
            lock (SyncRoot)
            {
                if (_messageReceivedEvent != null) _messageReceivedEvent.Dispose();
                _messageReceivedEvent = new CountdownEvent(1);
            }
        }
    }
}