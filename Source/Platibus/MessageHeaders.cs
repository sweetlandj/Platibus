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

        /// <inheritdoc/>
        public string this[HeaderName header]
        {
            get
            {
                _headers.TryGetValue(header, out string value);
                return value;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _headers.Remove(header);
                }
                else
                {
                    _headers[header] = value;
                }
            }
        }

        /// <inheritdoc/>
        public MessageId MessageId
        {
            get => this[HeaderName.MessageId];
            set => this[HeaderName.MessageId] = value;
        }

        /// <inheritdoc/>
        public MessageName MessageName
        {
            get => this[HeaderName.MessageName];
            set => this[HeaderName.MessageName] = value;
        }

        /// <inheritdoc/>
        public DateTime Expires
        {
            get => GetDateTime(HeaderName.Expires).GetValueOrDefault(DateTime.MaxValue);
            set => SetDateTime(HeaderName.Expires, value);
        }

        /// <inheritdoc/>
        public Uri Destination
        {
            get => GetUri(HeaderName.Destination);
            set => SetUri(HeaderName.Destination, value);
        }

        /// <inheritdoc/>
        public Uri Origination
        {
            get => GetUri(HeaderName.Origination);
            set => SetUri(HeaderName.Origination, value);
        }

        /// <inheritdoc/>
        public Uri ReplyTo
        {
            get => GetUri(HeaderName.ReplyTo);
            set => SetUri(HeaderName.ReplyTo, value);
        }

        /// <inheritdoc/>
        public MessageId RelatedTo
        {
            get => this[HeaderName.RelatedTo];
            set => this[HeaderName.RelatedTo] = value;
        }

        /// <inheritdoc/>
        public DateTime Published
        {
            get => GetDateTime(HeaderName.Published).GetValueOrDefault();
            set => SetDateTime(HeaderName.Published, value);
        }

        /// <inheritdoc/>
        public TopicName Topic
        {
            get => this[HeaderName.Topic];
            set => this[HeaderName.Topic] = value;
        }

        /// <inheritdoc/>
        public DateTime Sent
        {
            get => GetDateTime(HeaderName.Sent).GetValueOrDefault();
            set => SetDateTime(HeaderName.Sent, value);
        }

        /// <inheritdoc/>
        public DateTime Received
        {
            get => GetDateTime(HeaderName.Received).GetValueOrDefault();
            set => SetDateTime(HeaderName.Received, value);
        }

        /// <inheritdoc/>
        public string ContentType
        {
            get => this[HeaderName.ContentType];
            set => this[HeaderName.ContentType] = value;
        }

        /// <inheritdoc/>
        [Obsolete("Use Synchronous to override asynchronous queueing behavior")]
        public MessageImportance Importance
        {
            get => GetInt(HeaderName.Importance);
            set => SetInt(HeaderName.Importance, value);
        }

        /// <inheritdoc/>
        public bool Synchronous
        {
            get => GetBool(HeaderName.Synchronous);
            set => SetBool(HeaderName.Synchronous, value);
        }

        /// <inheritdoc/>
        public string SecurityToken
        {
            get => this[HeaderName.SecurityToken];
            set => this[HeaderName.SecurityToken] = value;
        }

        /// <summary>
        /// Returns the specified header value as a boolean value
        /// </summary>
        /// <param name="headerName">The name of the header</param>
        /// <returns>The specified header value as a boolean value, or false if the header 
        /// does not exist or could not be parsed as a boolean value.</returns>
        public bool GetBool(HeaderName headerName)
        {
            var value = this[headerName];
            return !string.IsNullOrWhiteSpace(value) &&
                   "true".Equals(value.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns the specified header value as a 32-bit integer value
        /// </summary>
        /// <param name="headerName">The name of the header</param>
        /// <returns>The specified header value as a 32-bit integer, or zero if the header 
        /// does not exist or could not be parsed to an integer</returns>
        public int GetInt(HeaderName headerName)
        {
            var value = this[headerName];
            if (!string.IsNullOrWhiteSpace(value) && int.TryParse(value, out int intValue))
            {
                return intValue;
            }
            return 0;
        }

        /// <summary>
        /// Removes the header with the specified name
        /// </summary>
        /// <param name="headerName">The name of the header to remove</param>
        public void Remove(HeaderName headerName)
        {
            _headers.Remove(headerName);
        }

        /// <inheritdoc/>
        public Uri GetUri(HeaderName headerName)
        {
            var value = this[headerName];
            return string.IsNullOrWhiteSpace(value) 
                ? null 
                : new Uri(value, UriKind.Absolute).WithTrailingSlash();
        }

        /// <inheritdoc/>
        public DateTime? GetDateTime(HeaderName headerName)
        {
            var value = this[headerName];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            }
            return null;
        }

        /// <inheritdoc/>
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
        /// <param name="boolValue">The header value</param>
        public void SetBool(HeaderName headerName, bool boolValue)
        {
            this[headerName] = boolValue.ToString();
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
            this[headerName] = uri?.AbsoluteUri;
        }

        /// <summary>
        /// Sets the value of a header
        /// </summary>
        /// <param name="headerName">The name of the header</param>
        /// <param name="dateTime">The header value</param>
        public void SetDateTime(HeaderName headerName, DateTime? dateTime)
        {
            this[headerName] = dateTime?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
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