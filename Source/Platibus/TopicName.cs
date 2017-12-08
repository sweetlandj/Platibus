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

namespace Platibus
{
    /// <summary>
    /// A name that identifies a topic to which messages can be published
    /// </summary>
    [DebuggerDisplay("{" + nameof(_value) + ",nq}")]
    public class TopicName : IEquatable<TopicName>
    {
        private readonly string _value;

        /// <summary>
        /// Initializes a new <see cref="TopicName"/> object with the
        /// specified string representation
        /// </summary>
        /// <param name="value">The string representation of the topic name</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/>
        /// is <c>null</c> or whitespace</exception>
        public TopicName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
            _value = value.Trim();
        }
        
        /// <summary>
        /// Determines whether another topic name is equal to this one
        /// </summary>
        /// <param name="topicName">The other topic name</param>
        /// <returns>Returns <c>true</c> if the other <paramref name="topicName"/>
        /// is equal to this one; <c>false</c> otherwise</returns>
        public bool Equals(TopicName topicName)
        {
            if (ReferenceEquals(null, topicName)) return false;
            return string.Equals(_value, topicName._value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns the string representation of the topic name
        /// </summary>
        /// <returns>Returns the string representation of the topic name</returns>
        public override string ToString()
        {
            return _value;
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
            return Equals(obj as TopicName);
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
            return _value.ToLowerInvariant().GetHashCode();
        }

        /// <summary>
        /// Overrides the <c>==</c> operator to determine object equality based on value
        /// rather than identity
        /// </summary>
        /// <param name="left">The topic name on the left side of the operator</param>
        /// <param name="right">The topic name on the right side of the operator</param>
        /// <returns>Returns <c>true</c> if the topic names are equal; <c>false</c> otherwise</returns>
        public static bool operator ==(TopicName left, TopicName right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Overrides the <c>!=</c> operator to determine object inequality based on value
        /// rather than identity
        /// </summary>
        /// <param name="left">The topic name on the left side of the operator</param>
        /// <param name="right">The topic name on the right side of the operator</param>
        /// <returns>Returns <c>true</c> if the topic names are unequal; <c>false</c> otherwise</returns>
        public static bool operator !=(TopicName left, TopicName right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Implicitly converts a string value to a <see cref="TopicName"/>
        /// </summary>
        /// <param name="value">The string representation of the topic name</param>
        /// <returns>Returns <c>null</c> if <paramref name="value"/> is <c>null</c> or
        /// whitespace; otherwise returns the <see cref="TopicName"/> for the specified
        /// string representation</returns>
        public static implicit operator TopicName(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : new TopicName(value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="TopicName"/> to its string representation
        /// </summary>
        /// <param name="topicName">The topic name</param>
        /// <returns>Returns <c>null</c> if <paramref name="topicName"/> is <c>null</c>;
        /// otherwise returns the string representation of the topic name</returns>
        public static implicit operator string(TopicName topicName)
        {
            return topicName == null ? null : topicName._value;
        }
    }
}