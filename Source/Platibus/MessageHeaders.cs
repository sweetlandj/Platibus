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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Platibus
{
    public class MessageHeaders : IMessageHeaders
    {
        private readonly IDictionary<HeaderName, string> _headers = new Dictionary<HeaderName, string>();

        public MessageHeaders()
        {
        }

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

        public MessageId MessageId
        {
            get { return this[HeaderName.MessageId]; }
            set { this[HeaderName.MessageId] = value; }
        }

        public MessageName MessageName
        {
            get { return this[HeaderName.MessageName]; }
            set { this[HeaderName.MessageName] = value; }
        }

        public DateTime Expires
        {
            get { return GetDateTime(HeaderName.Expires).GetValueOrDefault(DateTime.MaxValue); }
            set { SetDateTime(HeaderName.Expires, value); }
        }

        public Uri Destination
        {
            get { return GetUri(HeaderName.Destination); }
            set { SetUri(HeaderName.Destination, value); }
        }

        public Uri Origination
        {
            get { return GetUri(HeaderName.Origination); }
            set { SetUri(HeaderName.Origination, value); }
        }

        public Uri ReplyTo
        {
            get { return GetUri(HeaderName.ReplyTo); }
            set { SetUri(HeaderName.ReplyTo, value); }
        }

        public MessageId RelatedTo
        {
            get { return this[HeaderName.RelatedTo]; }
            set { this[HeaderName.RelatedTo] = value; }
        }

        public DateTime Published
        {
            get { return GetDateTime(HeaderName.Published).GetValueOrDefault(DateTime.UtcNow); }
            set { SetDateTime(HeaderName.Published, value); }
        }

        public TopicName Topic
        {
            get { return this[HeaderName.Topic]; }
            set { this[HeaderName.Topic] = value; }
        }

        public DateTime Sent
        {
            get { return GetDateTime(HeaderName.Sent).GetValueOrDefault(DateTime.UtcNow); }
            set { SetDateTime(HeaderName.Sent, value); }
        }

        public DateTime Received
        {
            get { return GetDateTime(HeaderName.Received).GetValueOrDefault(DateTime.UtcNow); }
            set { SetDateTime(HeaderName.Received, value); }
        }

        public string ContentType
        {
            get { return this[HeaderName.ContentType]; }
            set { this[HeaderName.ContentType] = value; }
        }

        public MessageImportance Importance
        {
            get { return GetInt(HeaderName.Importance); }
            set { SetInt(HeaderName.Importance, value); }
        }

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

        public Uri GetUri(HeaderName headerName)
        {
            var value = this[headerName];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return new Uri(value, UriKind.Absolute);
            }
            return null;
        }

        public DateTime? GetDateTime(HeaderName headerName)
        {
            var value = this[headerName];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            }
            return null;
        }

        public IEnumerator<KeyValuePair<HeaderName, string>> GetEnumerator()
        {
            return _headers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void SetInt(HeaderName headerName, int intValue)
        {
            this[headerName] = intValue.ToString("d");
        }

        public void SetUri(HeaderName headerName, Uri uri)
        {
            this[headerName] = uri == null ? null : uri.AbsoluteUri;
        }

        public void SetDateTime(HeaderName headerName, DateTime? dateTime)
        {
            this[HeaderName.Sent] = dateTime == null
                ? null
                : dateTime.Value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
        }

        public void Add(HeaderName headerName, string value)
        {
            this[headerName] = value;
        }
    }
}