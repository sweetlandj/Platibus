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
using System.Security.Principal;

namespace Platibus
{
    public class SubscriptionRequestReceivedEventArgs : EventArgs
    {
        private readonly SubscriptionRequestType _requestType;
        private readonly Uri _subscriber;
        private readonly TopicName _topic;
        private readonly TimeSpan _ttl;
        private readonly IPrincipal _senderPrincipal;

        public SubscriptionRequestType RequestType
        {
            get { return _requestType; }
        }

        public TopicName Topic
        {
            get { return _topic; }
        }

        public Uri Subscriber
        {
            get { return _subscriber; }
        }

        public TimeSpan TTL
        {
            get { return _ttl; }
        }

        public IPrincipal SenderPrincipal
        {
            get { return _senderPrincipal; }
        }

        public SubscriptionRequestReceivedEventArgs(SubscriptionRequestType requestType, TopicName topic, Uri subscriber,
            TimeSpan ttl, IPrincipal senderPrincipal)
        {
            if (topic == null) throw new ArgumentNullException("topic");
            if (subscriber == null) throw new ArgumentNullException("subscriber");
            _requestType = requestType;
            _topic = topic;
            _subscriber = subscriber;
            _ttl = ttl;
            _senderPrincipal = senderPrincipal;
        }

    }
}