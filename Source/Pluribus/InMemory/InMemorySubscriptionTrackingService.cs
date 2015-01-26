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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pluribus.InMemory
{
    /// <summary>
    /// Tracks subscriptions using data structures in memory.
    /// </summary>
    public class BasicSubscriptionTrackingService : ISubscriptionTrackingService
    {
        private readonly ConcurrentDictionary<TopicName, IEnumerable<ExpiringSubscription>> _subscriptions =
            new ConcurrentDictionary<TopicName, IEnumerable<ExpiringSubscription>>();

        public Task AddSubscription(TopicName topic, Uri subscriber, TimeSpan ttl = default(TimeSpan),
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var expirationDate = ttl <= TimeSpan.Zero ? DateTime.MaxValue : DateTime.UtcNow + ttl;
            var expiringSubscription = new ExpiringSubscription(subscriber, expirationDate);

            _subscriptions.AddOrUpdate(topic, new[] {expiringSubscription},
                (t, existing) => new[] {expiringSubscription}.Union(existing).ToList());

            return Task.FromResult(true);
        }

        public Task RemoveSubscription(TopicName topic, Uri subscriber, TimeSpan ttl = default(TimeSpan),
            CancellationToken cancellationToken = default(CancellationToken))
        {
            _subscriptions.AddOrUpdate(topic, new ExpiringSubscription[0],
                (t, existing) => existing.Where(se => se.Subscriber != subscriber).ToList());

            return Task.FromResult(true);
        }

        public IEnumerable<Uri> GetSubscribers(TopicName topicName)
        {
            IEnumerable<ExpiringSubscription> subscriptions;
            _subscriptions.TryGetValue(topicName, out subscriptions);
            return (subscriptions ?? Enumerable.Empty<ExpiringSubscription>())
                .Where(s => s.ExpirationDate > DateTime.UtcNow)
                .Select(s => s.Subscriber);
        }

        private class ExpiringSubscription : IEquatable<ExpiringSubscription>
        {
            private readonly DateTime _expirationDate;
            private readonly Uri _subscriber;

            public ExpiringSubscription(Uri subscriber, DateTime expirationDate)
            {
                _subscriber = subscriber;
                _expirationDate = expirationDate;
            }

            public Uri Subscriber
            {
                get { return _subscriber; }
            }

            public DateTime ExpirationDate
            {
                get { return _expirationDate; }
            }

            public bool Equals(ExpiringSubscription subscription)
            {
                if (ReferenceEquals(this, subscription)) return true;
                if (ReferenceEquals(null, subscription)) return false;
                return Equals(_subscriber, subscription._subscriber);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as ExpiringSubscription);
            }

            public override int GetHashCode()
            {
                return _subscriber.GetHashCode();
            }
        }
    }
}
