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
    /// An amplified value type that provides equality semantics for header names
    /// </summary>
    [DebuggerDisplay("{_value,nq}")]
    public class HeaderName : IEquatable<HeaderName>
    {
        private static readonly char[] InvalidChars =
        {
            '$', '.', '(', ')', '<', '>', '@', ',', ';', ':', '\\', '<', '>', '/', '[', ']',
            '?', '=', '{', '}', ' ', (char) 9
        };

        /// <summary>
        ///     The unique ID used to identify a received message.
        /// </summary>
        public static readonly HeaderName MessageId = "Platibus-MessageId";

        /// <summary>
        ///     The unqualified name of the type of message that was sent.
        /// </summary>
        public static readonly HeaderName MessageName = "Platibus-MessageName";

        /// <summary>
        ///     The ISO-8601 datetime after which the message expires and should bot be
        ///     processed.  If no offset is specified, UTC is assumed.
        /// </summary>
        public static readonly HeaderName Expires = "Platibus-Expires";

        /// <summary>
        ///     The base URI of the Platibus server to which the message is addressed.
        /// </summary>
        public static readonly HeaderName Destination = "Platibus-Destination";

        /// <summary>
        ///     The base URI of the Platibus server from which the message originated.
        /// </summary>
        public static readonly HeaderName Origination = "Platibus-Origination";

        /// <summary>
        ///     The base URI of the Platibus server to which replies should be directed
        /// </summary>
        public static readonly HeaderName ReplyTo = "Platibus-ReplyTo";

        /// <summary>
        ///     The message ID of a message to which this message is related.  This is
        ///     used to correlate replies, publications, and downstream messages that
        ///     are sent as a result of the related message.
        /// </summary>
        public static readonly HeaderName RelatedTo = "Platibus-RelatedTo";

        /// <summary>
        ///     The ISO-8601 datetime the message was originally published to a topic.  
        ///     If no offset is specified, UTC is assumed.
        /// </summary>
        public static readonly HeaderName Published = "Platibus-Published";

        /// <summary>
        ///     The topic to which the message was published.
        /// </summary>
        public static readonly HeaderName Topic = "Platibus-Topic";

        /// <summary>
        ///     The ISO-8601 datetime the message was sent.  If no offset is specified,
        ///     UTC is assumed.
        /// </summary>
        public static readonly HeaderName Sent = "Platibus-Sent";

        /// <summary>
        ///     The ISO-8601 datetime the message was received.  If no offset is specified,
        ///     UTC is assumed.
        /// </summary>
        public static readonly HeaderName Received = "Platibus-Received";

        /// <summary>
        ///     Content type.
        /// </summary>
        public static readonly HeaderName ContentType = "Content-Type";

        /// <summary>
        ///     Directives for the receiver regarding how the sender would like the
        ///     message to be processed.
        /// </summary>
        public static readonly HeaderName Importance = "Platibus-Importance";

        /// <summary>
        ///     Security token capturing the claims associated with the principal from which the
        ///     message was originally received.
        /// </summary>
        public static readonly HeaderName SecurityToken = "Platibus-SecurityToken";

        private readonly string _value;

        /// <summary>
        /// Initializes a new <see cref="HeaderName"/> with the specified <paramref name="value"/>
        /// </summary>
        /// <param name="value">The header name as a string</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is
        /// <c>null</c> or whitespace</exception>
        /// <exception cref="FormatException">Thrown if <paramref name="value"/> contains invalid
        /// characters</exception>
        /// <remarks>
        /// Header names may not contain the character ':'
        /// </remarks>
        public HeaderName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("value");
            foreach(var c in InvalidChars)
            {
                if (value.IndexOf(c) >= 0) throw new FormatException("Invalid character '" + c + "'");
            }
            _value = value.Trim();
        }

        /// <summary>
        /// Determines whether another <paramref name="headerName"/> is equal to this one
        /// </summary>
        /// <param name="headerName">The other header name</param>
        /// <returns>Returns <c>true</c> if the other <paramref name="headerName"/> is equal
        /// to this one; <c>false</c> otherwise</returns>
        public bool Equals(HeaderName headerName)
        {
            return !ReferenceEquals(null, headerName) && 
                string.Equals(_value, headerName._value, StringComparison.InvariantCultureIgnoreCase);
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
            return Equals(obj as HeaderName);
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
        /// Overloaded equality operator used to determine whether two <see cref="HeaderName"/>
        /// objects are equal
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>Returns <c>true</c> if the two header names are equal; <c>false</c>
        /// otherwise</returns>
        public static bool operator ==(HeaderName left, HeaderName right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Overloaded inequality operator used to determine whether two <see cref="HeaderName"/>
        /// objects are not equal
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>Returns <c>true</c> if the two header names are not equal; <c>false</c>
        /// otherwise</returns>
        public static bool operator !=(HeaderName left, HeaderName right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Implicitly converts a string into a <see cref="HeaderName"/>
        /// </summary>
        /// <param name="value">The header name as a string</param>
        /// <returns>Returns <c>null</c> if <paramref name="value"/> is <c>null</c> or
        /// whitespace; otherwise returns a new <see cref="HeaderName"/> instance
        /// with the specified <paramref name="value"/></returns>
        public static implicit operator HeaderName(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : new HeaderName(value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="HeaderName"/> object to a string
        /// </summary>
        /// <param name="headerName">The header name object</param>
        /// <returns>Returns <c>null</c> if <paramref name="headerName"/> is <c>null</c>;
        /// otherwise returns the value of the <paramref name="headerName"/> object</returns>
        public static implicit operator string(HeaderName headerName)
        {
            return headerName == null ? null : headerName._value;
        }
    }
}