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
    /// <inheritdoc cref="IComparable{T}"/>
    /// <summary>
    /// An indication of how important a message is and therefore how it should be
    /// handled by both the sender and the recipient
    /// </summary>
    [Obsolete("Use SendOptions.Synchronous to override asynchronous queueing behavior")]
    [DebuggerDisplay("{" + nameof(_value) + ",nq}")]
    public struct MessageImportance : IComparable<MessageImportance>
    {
        private const int LowValue = -1;
        private const int NormalValue = 0;
        private const int HighValue = 1;
        private const int CriticalValue = 2;

        /// <summary>
        /// Low importance.  Can be discarded in cases of extreme congestion.
        /// </summary>
        public static readonly MessageImportance Low = new MessageImportance(LowValue);

        /// <summary>
        /// Normal importance.  Lowest priority for which discarding the message is not
        /// an option.  A best effort attempt must be made to process the message, but
        /// the receiver is not required to queue the message or retry processing.
        /// </summary>
        public static readonly MessageImportance Normal = new MessageImportance(NormalValue);

        /// <summary>
        /// High importance.  Messages should be prioritized over messages with <see cref="LowValue"/> 
        /// and <see cref="NormalValue"/> importance and should never be discarded.  A best effort
        /// attempt must be made to process the message, but the receiver is not required to
        /// queue the message or retry processing.
        /// </summary>
        public static readonly MessageImportance High = new MessageImportance(HighValue);

        /// <summary>
        /// Critical importance.  The message must be queued and processed at least once.
        /// </summary>
        public static readonly MessageImportance Critical = new MessageImportance(CriticalValue);

        private readonly int _value;

        /// <summary>
        /// Indicates whether this message importance level allows messages to be discarded
        /// </summary>
        public bool AllowsDiscarding => _value <= LowValue;

        /// <summary>
        /// Indicates whether this message importance level requires messages to be queued
        /// </summary>
        public bool RequiresQueueing => _value >= CriticalValue;

        /// <summary>
        /// Initializes a new <see cref="MessageImportance"/> with the specified relative value
        /// </summary>
        /// <param name="value"></param>
        public MessageImportance(int value)
        {
            _value = value;
        }

        /// <summary>
        /// Determines whether another <paramref name="importance"/> is equal to this one
        /// </summary>
        /// <param name="importance">The other message importance</param>
        /// <returns>Returns <c>true</c> if the other message importance is equal to this one;
        /// <c>false</c> otherwise</returns>
        public bool Equals(MessageImportance importance)
        {
            return _value == importance._value;
        }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing a fully qualified type name.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return _value.ToString("d");
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">Another object to compare to. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            return obj is MessageImportance && Equals((MessageImportance) obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return _value;
        }

        /// <summary>
        /// Overloaded <c>==</c> operator that determines the equality of two importance
        /// levels based on value rather than by reference
        /// </summary>
        /// <param name="left">The message importance on the left side of the operator</param>
        /// <param name="right">The message importance on the right side of the operator</param>
        /// <returns>Returns <c>true</c> if the two message importance levels are equal;
        /// <c>false</c> otherwise</returns>
        public static bool operator ==(MessageImportance left, MessageImportance right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Overloaded <c>!=</c> operator that determines the inequality of two importance
        /// levels based on value rather than by reference
        /// </summary>
        /// <param name="left">The message importance on the left side of the operator</param>
        /// <param name="right">The message importance on the right side of the operator</param>
        /// <returns>Returns <c>true</c> if the two message importance levels are unequal;
        /// <c>false</c> otherwise</returns>
        public static bool operator !=(MessageImportance left, MessageImportance right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Implicitly cases an integer value into a <see cref="MessageImportance"/> object
        /// </summary>
        /// <param name="value">The integer value</param>
        /// <returns>Returns a <see cref="MessageImportance"/> corresponding to the specified
        /// <paramref name="value"/></returns>
        public static implicit operator MessageImportance(int value)
        {
            return new MessageImportance(value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="MessageImportance"/> to an integer
        /// </summary>
        /// <param name="importance">The message importance</param>
        /// <returns>Returns the integer value of the message importance level</returns>
        public static implicit operator int(MessageImportance importance)
        {
            return importance._value;
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(MessageImportance other)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return _value.CompareTo(other._value);
        }
    }
}