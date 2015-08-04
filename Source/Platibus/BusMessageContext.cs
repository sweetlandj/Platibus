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
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus
{
    internal class BusMessageContext : IMessageContext
    {
        private readonly Bus _bus;
        private readonly IMessageHeaders _headers;
        private readonly IPrincipal _senderPrincipal;

        public BusMessageContext(Bus bus, IMessageHeaders headers, IPrincipal senderPrincipal)
        {
            if (bus == null) throw new ArgumentNullException("bus");
            if (headers == null) throw new ArgumentNullException("headers");
            _bus = bus;
            _headers = headers;
            _senderPrincipal = senderPrincipal;
        }

        public IMessageHeaders Headers
        {
            get { return _headers; }
        }

        public bool MessageAcknowledged { get; private set; }

        public IBus Bus
        {
            get { return _bus; }
        }

        public IPrincipal SenderPrincipal
        {
            get { return _senderPrincipal; }
        }

        public void Acknowledge()
        {
            MessageAcknowledged = true;
        }

        public Task SendReply(object replyContent, SendOptions options = default(SendOptions),
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (replyContent == null) throw new ArgumentNullException("replyContent");
            return _bus.SendReply(this, replyContent, options, cancellationToken);
        }
    }
}