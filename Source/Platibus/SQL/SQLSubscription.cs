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

namespace Platibus.SQL
{
    /// <summary>
    /// An immutable representation of a subscription used by the 
    /// <see cref="SQLSubscriptionTrackingService"/>
    /// </summary>
    public class SQLSubscription : IEquatable<SQLSubscription>
    {
        private readonly TopicName _topicName;
        private readonly Uri _subscriber;
        private readonly DateTime _expires;

        /// <summary>
        /// Initializes a new <see cref="SQLSubscription"/> with the specified topic,
        /// subscriber URI, and expiration date
        /// </summary>
        /// <param name="topicName">The name of the topic to which the subscriber is
        /// subscribing</param>
        /// <param name="subscriber">The base URI of the subscriber</param>
        /// <param name="expires">The date and time at which the subscription expires</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="topicName"/>
        /// or <paramref name="subscriber"/> is <c>null</c></exception>
        public SQLSubscription(TopicName topicName, Uri subscriber, DateTime expires)
        {
            if (topicName == null) throw new ArgumentNullException("topicName");
            if (subscriber == null) throw new ArgumentNullException("subscriber");
            _topicName = topicName;
            _subscriber = subscriber;
            _expires = expires;
        }

        /// <summary>
        /// The name of the topic to which the subscriber is subscribing
        /// </summary>
        public TopicName TopicName
        {
            get { return _topicName; }
        }

        /// <summary>
        /// The base URI of the subscribing application
        /// </summary>
        public Uri Subscriber
        {
            get { return _subscriber; }
        }

        /// <summary>
        /// The date and time at which the subscription expires
        /// </summary>
        public DateTime Expires
        {
            get { return _expires; }
        }

        /// <summary>
        /// Determines whether another SQL subscription is equal to this one
        /// </summary>
        /// <param name="subscription">The other SQL subscription</param>
        /// <returns>Returns <c>true</c> if the other subscription is equal to
        /// this one; <c>false</c> otherwise</returns>
        public bool Equals(SQLSubscription subscription)
        {
            if (ReferenceEquals(this, subscription)) return true;
            if (ReferenceEquals(null, subscription)) return false;
            return Equals(_topicName, subscription._topicName) && Equals(_subscriber, subscription._subscriber);
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
            return Equals(obj as SQLSubscription);
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
            return _subscriber.GetHashCode();
        }
    }
}