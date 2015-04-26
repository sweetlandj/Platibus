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
using System.Diagnostics;

namespace Platibus
{
    [DebuggerDisplay("{_value,nq}")]
    public class HeaderName : IEquatable<HeaderName>
    {
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
        ///     Message durability settings
        /// </summary>
        public static readonly HeaderName Durability = "Platibus-Durability";

        private readonly string _value;

        public HeaderName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("value");
            if (value.IndexOf(':') >= 0) throw new FormatException("Header name may not contain ':'");
            _value = value.Trim();
        }

        public bool Equals(HeaderName headerName)
        {
            if (ReferenceEquals(null, headerName)) return false;
            return string.Equals(_value, headerName._value, StringComparison.InvariantCultureIgnoreCase);
        }

        public override string ToString()
        {
            return _value;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as HeaderName);
        }

        public override int GetHashCode()
        {
            return _value.ToLowerInvariant().GetHashCode();
        }

        public static bool operator ==(HeaderName left, HeaderName right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(HeaderName left, HeaderName right)
        {
            return !Equals(left, right);
        }

        public static implicit operator HeaderName(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : new HeaderName(value);
        }

        public static implicit operator string(HeaderName headerName)
        {
            return headerName == null ? null : headerName._value;
        }
    }
}