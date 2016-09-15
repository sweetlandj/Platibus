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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Platibus
{
    /// <summary>
    /// Mutable implementation of <see cref="IMessageHeaders"/>
    ///</summary>
    public class MessageHeaders : IMessageHeaders
    {
        private readonly IDictionary<HeaderName, string> _headers = new Dictionary<HeaderName, string>();

        /// <summary>
        /// Initializes a new, empty <see cref="MessageHeaders"/> instance
        /// </summary>
        public MessageHeaders()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="MessageHeaders"/> with the specified header values
        /// </summary>
        /// <param name="headers">The values to add to the new <see cref="MessageHeaders"/>
        /// instance</param>
        public MessageHeaders(IEnumerable<KeyValuePair<HeaderName, string>> headers)
        {
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    this[header.Key] = header.Value;
                }
            }
        }

        /// <summary>
        /// Initializes a new <see cref="MessageHeaders"/> with the specified header values
        /// </summary>
        /// <param name="headers">The values to add to the new <see cref="MessageHeaders"/>
        /// instance</param>
        public MessageHeaders(IEnumerable<KeyValuePair<string, string>> headers)
        {
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    this[header.Key] = header.Value;
                }
            }
        }

        /// <summary>
        /// Returns the value of the specified <paramref name="header"/>
        /// as a string
        /// </summary>
        /// <param name="header">The name of the header</param>
        /// <returns>The value of the specified header, or <c>null</c>
        /// if that header is not present</returns>
        public string this[HeaderName header]
        {
            get
            {
                string value;
                _headers.TryGetValue(header, out value);
                return value;
            }
            set { _headers[header] = value; }
        }

        /// <summary>
        /// Returns the unique identifier associated with the message
        /// </summary>
        public MessageId MessageId
        {
            get { return this[HeaderName.MessageId]; }
            set { this[HeaderName.MessageId] = value; }
        }

        /// <summary>
        /// Returns the name of the message
        /// </summary>
        /// <remarks>
        /// The message name is used to identify an appropriate type for
        /// content deserialization
        /// </remarks>
        /// <seealso cref="IMessageNamingService"/>
        public MessageName MessageName
        {
            get { return this[HeaderName.MessageName]; }
            set { this[HeaderName.MessageName] = value; }
        }

        /// <summary>
        /// Returns the date and time at which the message expires
        /// </summary>
        /// <remarks>
        /// Expired messages should not be handled (UTC)
        /// </remarks>
        public DateTime Expires
        {
            get { return GetDateTime(HeaderName.Expires).GetValueOrDefault(DateTime.MaxValue); }
            set { SetDateTime(HeaderName.Expires, value); }
        }

        /// <summary>
        /// Returns the base URI of the endpoint to which the message
        /// was or is to be sent
        /// </summary>
        public Uri Destination
        {
            get { return GetUri(HeaderName.Destination); }
            set { SetUri(HeaderName.Destination, value); }
        }

        /// <summary>
        /// Returns the base URI of the endpoint from which the message
        /// was sent
        /// </summary>
        public Uri Origination
        {
            get { return GetUri(HeaderName.Origination); }
            set { SetUri(HeaderName.Origination, value); }
        }

        /// <summary>
        /// Returns the base URI of the endpoint to which replies should
        /// be sent (if different than the <see cref="IMessageHeaders.Origination"/>)
        /// </summary>
        public Uri ReplyTo
        {
            get { return GetUri(HeaderName.ReplyTo); }
            set { SetUri(HeaderName.ReplyTo, value); }
        }

        /// <summary>
        /// Returns the message ID of the message to which this message
        /// is a reply
        /// </summary>
        public MessageId RelatedTo
        {
            get { return this[HeaderName.RelatedTo]; }
            set { this[HeaderName.RelatedTo] = value; }
        }

        /// <summary>
        /// Returns the date and time the message was published (UTC)
        /// </summary>
        public DateTime Published
        {
            get { return GetDateTime(HeaderName.Published).GetValueOrDefault(DateTime.UtcNow); }
            set { SetDateTime(HeaderName.Published, value); }
        }

        /// <summary>
        /// Returns the name of the topic to which the message was published
        /// </summary>
        public TopicName Topic
        {
            get { return this[HeaderName.Topic]; }
            set { this[HeaderName.Topic] = value; }
        }

        /// <summary>
        /// Returns the date and time the message was sent (UTC)
        /// </summary>
        public DateTime Sent
        {
            get { return GetDateTime(HeaderName.Sent).GetValueOrDefault(DateTime.UtcNow); }
            set { SetDateTime(HeaderName.Sent, value); }
        }

        /// <summary>
        /// Returns the date and time the message was received (UTC)
        /// </summary>
        public DateTime Received
        {
            get { return GetDateTime(HeaderName.Received).GetValueOrDefault(DateTime.UtcNow); }
            set { SetDateTime(HeaderName.Received, value); }
        }

        /// <summary>
        /// Returns the MIME type of the message content
        /// </summary>
        public string ContentType
        {
            get { return this[HeaderName.ContentType]; }
            set { this[HeaderName.ContentType] = value; }
        }

        /// <summary>
        /// Returns the importance of the message
        /// </summary>
        public MessageImportance Importance
        {
            get { return GetInt(HeaderName.Importance); }
            set { SetInt(HeaderName.Importance, value); }
        }

        /// <summary>
        /// Returns the specified header value as a 32-bit integer value
        /// </summary>
        /// <param name="headerName">The name of the header</param>
        /// <returns>The specified header value as a 32-bit integer, or zero if the header 
        /// does not exist or could not be parsed to an integer</returns>
        public int GetInt(HeaderName headerName)
        {
            int intValue;
            var value = this[headerName];
            if (!string.IsNullOrWhiteSpace(value) && int.TryParse(value, out intValue))
            {
                return intValue;
            }
            return 0;
        }

        /// <summary>
        /// Returns the specified header value as a URI
        /// </summary>
        /// <param name="headerName">The name of the header</param>
        /// <returns>Returns the specified header value as URI if it exists,
        /// <c>null</c> otherwise</returns>
        public Uri GetUri(HeaderName headerName)
        {
            var value = this[headerName];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return new Uri(value, UriKind.Absolute);
            }
            return null;
        }

        /// <summary>
        /// Returns the specified header value as a UTC date/time
        /// </summary>
        /// <param name="headerName">The name of the header</param>
        /// <returns>Returns the specified header value as UTC date/time if 
        /// it exists, <c>null</c> otherwise</returns>
        public DateTime? GetDateTime(HeaderName headerName)
        {
            var value = this[headerName];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            }
            return null;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<HeaderName, string>> GetEnumerator()
        {
            return _headers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Sets the value of a header
        /// </summary>
        /// <param name="headerName">The name of the header</param>
        /// <param name="intValue">The header value</param>
        public void SetInt(HeaderName headerName, int intValue)
        {
            this[headerName] = intValue.ToString("d");
        }

        /// <summary>
        /// Sets the value of a header
        /// </summary>
        /// <param name="headerName">The name of the header</param>
        /// <param name="uri">The header value</param>
        public void SetUri(HeaderName headerName, Uri uri)
        {
            this[headerName] = uri == null ? null : uri.AbsoluteUri;
        }

        /// <summary>
        /// Sets the value of a header
        /// </summary>
        /// <param name="headerName">The name of the header</param>
        /// <param name="dateTime">The header value</param>
        public void SetDateTime(HeaderName headerName, DateTime? dateTime)
        {
            this[HeaderName.Sent] = dateTime == null
                ? null
                : dateTime.Value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Sets the value of a header
        /// </summary>
        /// <param name="headerName">The name of the header</param>
        /// <param name="value">The header value</param>
        /// <remarks>
        /// This method exists to support the list initializer syntax
        /// </remarks>
        public void Add(HeaderName headerName, string value)
        {
            this[headerName] = value;
        }
    }
}