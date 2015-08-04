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
    /// A globally unique vaue that identifies a message
    /// </summary>
    [DebuggerDisplay("{_value,nq}")]
    public struct MessageId : IEquatable<MessageId>
    {
        private readonly Guid _value;

        /// <summary>
        /// Initializes a new <see cref="MessageId"/> with the specified
        /// GUID value
        /// </summary>
        /// <param name="value">A GUID value</param>
        public MessageId(Guid value)
        {
            _value = value;
        }

        /// <summary>
        /// Indicates whether another <paramref name="messageId"/> instance
        /// is equal to this one
        /// </summary>
        /// <param name="messageId">The other message ID</param>
        /// <returns>Returns <c>true</c> if the other <paramref name="messageId"/>
        /// is equal to this one; <c>false</c> otherwise.</returns>
        public bool Equals(MessageId messageId)
        {
            return _value == messageId._value;
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
            return _value.ToString();
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
            return (obj is MessageId) && Equals((MessageId) obj);
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
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return _value.GetHashCode();
        }

        public static bool operator ==(MessageId left, MessageId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MessageId left, MessageId right)
        {
            return !Equals(left, right);
        }

        public static implicit operator MessageId(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? new MessageId() : new MessageId(Guid.Parse(value));
        }

        public static implicit operator string(MessageId messageId)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return messageId._value.ToString();
        }

        public static implicit operator MessageId(Guid value)
        {
            return new MessageId(value);
        }

        public static implicit operator Guid(MessageId messageId)
        {
            return messageId == null ? Guid.Empty : messageId._value;
        }

        /// <summary>
        /// Factory method for generating new message IDs
        /// </summary>
        /// <returns>A new message ID</returns>
        public static MessageId Generate()
        {
            return new MessageId(Guid.NewGuid());
        }
    }
}