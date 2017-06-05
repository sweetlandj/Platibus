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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Platibus.InMemory;

namespace Platibus.UnitTests.Stubs
{
    public class SubscriptionTrackingServiceStub : ISubscriptionTrackingService
    {
        private readonly InMemorySubscriptionTrackingService _storage = new InMemorySubscriptionTrackingService();

        public event SubscriptionAddedEventHandler SubscriptionAdded;
        public event SubscriptionRemovedEventHandler SubscriptionRemoved;

        public Task AddSubscription(TopicName topic, Uri subscriber, TimeSpan ttl = new TimeSpan(),
            CancellationToken cancellationToken = new CancellationToken())
        {
            var result = _storage.AddSubscription(topic, subscriber, ttl, cancellationToken);
            var handlers = SubscriptionAdded;
            if (handlers != null)
            {
                handlers(this, new SubscriptionAddedEventArgs(topic, subscriber, ttl));
            }
            return result;
        }

        public Task RemoveSubscription(TopicName topic, Uri subscriber, CancellationToken cancellationToken = new CancellationToken())
        {
            var result = _storage.RemoveSubscription(topic, subscriber, cancellationToken);
            var handlers = SubscriptionRemoved;
            if (handlers != null)
            {
                handlers(this, new SubscriptionRemovedEventArgs(topic, subscriber));
            }
            return result;
        }

        public Task<IEnumerable<Uri>> GetSubscribers(TopicName topic, CancellationToken cancellationToken = new CancellationToken())
        {
            return _storage.GetSubscribers(topic, cancellationToken);
        }
    }
}
