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
using System.Diagnostics;

namespace Platibus
{
    /// <summary>
    /// An amplified value type that provides equality semantics for queue names
    /// </summary>
    [DebuggerDisplay("{_value,nq}")]
    public class QueueName : IEquatable<QueueName>
    {
        private readonly string _value;

        /// <summary>
        /// Initializes a new <see cref="QueueName"/> with the specified string
        /// representation
        /// </summary>
        /// <param name="value">The string representation of the queue name</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/>
        /// is <c>null</c> or whitespace</exception>
        public QueueName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("value");
            _value = value.Trim();
        }

        /// <summary>
        /// Determines whether another queue name is equal to this one
        /// </summary>
        /// <param name="queueName">The other queue name</param>
        /// <returns>Returns <c>true</c> if the other queue name is equal to this
        /// one; <c>false</c> otherwise</returns>
        public bool Equals(QueueName queueName)
        {
            if (ReferenceEquals(null, queueName)) return false;
            return string.Equals(_value, queueName._value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
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
            return Equals(obj as QueueName);
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
        /// Overloaded operator used to determine whether two queue names are
        /// equal based on their respective values.
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>Returns true of the left and right operands represent the
        /// same queue name; <c>false</c> otherwise</returns>
        public static bool operator ==(QueueName left, QueueName right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Overloaded operator used to determine whether two queue names are
        /// unequal based on their respective values.
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>Returns true of the left and right operands do not represent
        /// the same queue name; <c>false</c> otherwise</returns>
        public static bool operator !=(QueueName left, QueueName right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Implicitly converts a string value into a <see cref="QueueName"/>
        /// object
        /// </summary>
        /// <param name="value">The string representation of the queue name</param>
        /// <returns>Returns <c>null</c> if <paramref name="value"/> is <c>null</c>
        /// or whitespace.  Otherwise returns a new <see cref="QueueName"/> object
        /// corresponding to the specified string <paramref name="value"/>.</returns>
        public static implicit operator QueueName(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : new QueueName(value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="QueueName"/> object into its
        /// string representation
        /// </summary>
        /// <param name="queueName">The queue name object</param>
        /// <returns>Returns <c>null</c> if <paramref name="queueName"/> is
        /// <c>null</c>.  Otherwise, returns the string representation of the
        /// specified <paramref name="queueName"/>.</returns>
        public static implicit operator string(QueueName queueName)
        {
            return queueName == null ? null : queueName._value;
        }
    }
}