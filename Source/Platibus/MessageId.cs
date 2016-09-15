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

        /// <summary>
        /// Overrides the default <c>==</c> operator so that equality is determined based on
        /// value rather than identity
        /// </summary>
        /// <param name="left">The message ID on the left side of the operator</param>
        /// <param name="right">The message ID on the right side of the operator</param>
        /// <returns>Returns <c>true</c> if the message IDs are equal; <c>false</c> otherwise</returns>
        public static bool operator ==(MessageId left, MessageId right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Overrides the default <c>!=</c> operator so that inequality is determined based on
        /// value rather than identity
        /// </summary>
        /// <param name="left">The message ID on the left side of the operator</param>
        /// <param name="right">The message ID on the right side of the operator</param>
        /// <returns>Returns <c>true</c> if the message IDs are unequal; <c>false</c> otherwise</returns>
        public static bool operator !=(MessageId left, MessageId right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Implicitly converts a string value to a <see cref="MessageId"/>
        /// </summary>
        /// <param name="value">The string value</param>
        /// <returns>Returns an empty message ID if <paramref name="value"/> is <c>null</c>
        /// or whitespace; otherwise returns a new <see cref="MessageId"/> based on the 
        /// value</returns>
        /// <exception cref="FormatException">Thrown if <paramref name="value"/> is not a valid
        /// message ID</exception>
        public static implicit operator MessageId(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? new MessageId() : new MessageId(Guid.Parse(value));
        }

        /// <summary>
        /// Implicitly converts a <see cref="MessageId"/> to its string representation
        /// </summary>
        /// <param name="messageId">The message ID</param>
        /// <returns>Returns the string value of the message ID</returns>
        public static implicit operator string(MessageId messageId)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return messageId._value.ToString();
        }

        /// <summary>
        /// Implicitly converts a GUID value to a <see cref="MessageId"/>
        /// </summary>
        /// <param name="value">The GUID value</param>
        /// <returns>Returns <c>null</c> if <paramref name="value"/> is <c>null</c> or whitespace;
        /// otherwise returns a new <see cref="MessageId"/> based on the value</returns>
        public static implicit operator MessageId(Guid value)
        {
            return new MessageId(value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="MessageId"/> to its GUID representation
        /// </summary>
        /// <param name="messageId">The message ID</param>
        /// <returns>Returns the GUID value of the message ID</returns>
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