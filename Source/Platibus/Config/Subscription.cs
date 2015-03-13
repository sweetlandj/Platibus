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
using System.Diagnostics;

namespace Platibus.Config
{
    [DebuggerDisplay("{ToString,nq}")]
    public class Subscription : ISubscription, IEquatable<Subscription>
    {
        private readonly EndpointName _endpoint;
        private readonly TopicName _topic;
        private readonly TimeSpan _ttl;

        public Subscription(EndpointName endpoint, TopicName topic, TimeSpan ttl)
        {
            if (endpoint == null) throw new ArgumentNullException("endpoint");
            if (topic == null) throw new ArgumentNullException("topic");
            if (ttl <= TimeSpan.Zero)
            {
                ttl = TimeSpan.FromHours(24);
            }
            _topic = topic;
            _endpoint = endpoint;
            _ttl = ttl;
        }

        public bool Equals(Subscription subscription)
        {
            return subscription != null
                   && _endpoint.Equals(subscription._endpoint)
                   && _topic.Equals(subscription._topic);
        }

        public EndpointName Publisher
        {
            get { return _endpoint; }
        }

        public TopicName Topic
        {
            get { return _topic; }
        }

        public TimeSpan TTL
        {
            get { return _ttl; }
        }

        public override string ToString()
        {
            return string.Format("{0}@{1}", _topic, _endpoint);
        }

        public override int GetHashCode()
        {
            var hashCode = _endpoint.GetHashCode();
            hashCode = (hashCode*397) ^ _topic.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Subscription);
        }

        public static bool operator ==(Subscription left, Subscription right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Subscription left, Subscription right)
        {
            return !Equals(left, right);
        }
    }
}