// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
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
    /// <summary>
    /// A basic implementation of <see cref="ISubscription"/>
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + ",nq}")]
    public class Subscription : ISubscription, IEquatable<Subscription>
    {
        /// <summary>
        /// Initializes a new <see cref="Subscription"/> to the specified
        /// <paramref name="endpoint"/> and <paramref name="topic"/> with the
        /// specified <paramref name="ttl"/>
        /// </summary>
        /// <param name="endpoint">The endpoint in which the <paramref name="topic"/>
        /// is hosted</param>
        /// <param name="topic">The topic being subscribed to</param>
        /// <param name="ttl">Optional.  The maximum amount of time the subscription will
        /// be effective unless it is renewed</param>
        /// <exception cref="ArgumentNullException">If <paramref name="endpoint"/> or
        /// <paramref name="topic"/> are <c>null</c></exception>
        public Subscription(EndpointName endpoint, TopicName topic, TimeSpan ttl = default(TimeSpan))
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            
            Topic = topic;
            Endpoint = endpoint;
            TTL = ttl;
        }

        /// <summary>
        /// Indicates whether another subscription is equal to this one
        /// </summary>
        /// <param name="subscription">The other subscription</param>
        /// <returns>
        /// Returns <c>true</c> if the current object is equal to the other 
        /// <paramref name="subscription"/>; <c>false</c> otherwise
        /// </returns>
        public bool Equals(Subscription subscription)
        {
            return subscription != null
                   && Endpoint.Equals(subscription.Endpoint)
                   && Topic.Equals(subscription.Topic);
        }

        /// <summary>
        /// The name of the publisher endpoint
        /// </summary>
        public EndpointName Endpoint { get; }

        /// <summary>
        /// The name of the topic
        /// </summary>
        public TopicName Topic { get; }

        /// <summary>
        /// The Time-To-Live (TTL) for the subscription
        /// </summary>
        /// <remarks>
        /// Subscriptions will regularly be renewed, but the TTL serves as a
        /// "dead man's switch" that will cause the subscription to be terminated
        /// if not renewed within that span of time.
        /// </remarks>
        public TimeSpan TTL { get; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return $"{Topic}@{Endpoint}";
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            var hashCode = Endpoint.GetHashCode();
            hashCode = (hashCode*397) ^ Topic.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            return Equals(obj as Subscription);
        }

        /// <summary>
        /// Overrides the default <c>==</c> operator to determine the equality of two subscriptions
        /// based on value rather than identity
        /// </summary>
        /// <param name="left">The subscription on the left side of the operator</param>
        /// <param name="right">The subscription on the right side of the operator</param>
        /// <returns>Returns <c>true</c> if the subscriptions are equal; <c>false</c> otherwise</returns>
        public static bool operator ==(Subscription left, Subscription right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Overrides the default <c>!=</c> operator to determine the inequality of two subscriptions
        /// based on value rather than identity
        /// </summary>
        /// <param name="left">The subscription on the left side of the operator</param>
        /// <param name="right">The subscription on the right side of the operator</param>
        /// <returns>Returns <c>true</c> if the subscriptions are unequal; <c>false</c> otherwise</returns>
        public static bool operator !=(Subscription left, Subscription right)
        {
            return !Equals(left, right);
        }
    }
}