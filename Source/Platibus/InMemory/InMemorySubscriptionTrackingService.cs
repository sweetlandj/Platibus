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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.InMemory
{
    /// <summary>
    /// Tracks subscriptions using data structures in memory.
    /// </summary>
    public class InMemorySubscriptionTrackingService : ISubscriptionTrackingService
    {
        private readonly ConcurrentDictionary<TopicName, IEnumerable<ExpiringSubscription>> _subscriptions =
            new ConcurrentDictionary<TopicName, IEnumerable<ExpiringSubscription>>();

        /// <summary>
        /// Adds or updates a subscription
        /// </summary>
        /// <param name="topic">The topic to which the <paramref name="subscriber"/> is
        /// subscribing</param>
        /// <param name="subscriber">The base URI of the subscribing Platibus instance</param>
        /// <param name="ttl">(Optional) The maximum Time To Live (TTL) for the subscription</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by
        /// the caller to cancel the addition of the subscription</param>
        /// <returns>Returns a task that will complete when the subscription has been added or
        /// updated</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="topic"/> or
        /// <paramref name="subscriber"/> is <c>null</c></exception>
        public Task AddSubscription(TopicName topic, Uri subscriber, TimeSpan ttl = default(TimeSpan),
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var expirationDate = ttl <= TimeSpan.Zero ? DateTime.MaxValue : DateTime.UtcNow + ttl;
            var expiringSubscription = new ExpiringSubscription(subscriber, expirationDate);

            _subscriptions.AddOrUpdate(topic, new[] {expiringSubscription},
                (t, existing) => new[] {expiringSubscription}.Union(existing).ToList());

            return Task.FromResult(true);
        }

        /// <summary>
        /// Removes a subscription
        /// </summary>
        /// <param name="topic">The topic to which the <paramref name="subscriber"/> is
        /// subscribing</param>
        /// <param name="subscriber">The base URI of the subscribing Platibus instance</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by
        /// the caller to cancel the subscription removal</param>
        /// <returns>Returns a task that will complete when the subscription has been removed</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="topic"/> or
        /// <paramref name="subscriber"/> is <c>null</c></exception>
        public Task RemoveSubscription(TopicName topic, Uri subscriber,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            _subscriptions.AddOrUpdate(topic, new ExpiringSubscription[0],
                (t, existing) => existing.Where(se => se.Subscriber != subscriber).ToList());

            return Task.FromResult(true);
        }

        /// <summary>
        /// Returns a list of the current, non-expired subscriber URIs for a topic
        /// </summary>
        /// <param name="topic">The topic</param>
        /// <param name="cancellationToken">A cancellation token that can be used by the caller
        /// to cancel the query</param>
        /// <returns>Returns a task whose result is the distinct set of base URIs of all Platibus
        /// instances subscribed to the specified local topic</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="topic"/> is <c>null</c>
        /// </exception>
        public Task<IEnumerable<Uri>> GetSubscribers(TopicName topic,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<ExpiringSubscription> subscriptions;
            _subscriptions.TryGetValue(topic, out subscriptions);
            var activeSubscriptions = (subscriptions ?? Enumerable.Empty<ExpiringSubscription>())
                .Where(s => s.ExpirationDate > DateTime.UtcNow)
                .Select(s => s.Subscriber);

            return Task.FromResult(activeSubscriptions);
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