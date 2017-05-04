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
using System.Collections.Generic;
using Platibus.Security;

namespace Platibus
{
    /// <summary>
    /// Interface providing access to the metadata transmitted with the
    /// message content
    /// </summary>
    public interface IMessageHeaders : IEnumerable<KeyValuePair<HeaderName, string>>
    {
        /// <summary>
        /// Returns the value of the specified <paramref name="header"/>
        /// as a string
        /// </summary>
        /// <param name="header">The name of the header</param>
        /// <returns>The value of the specified header, or <c>null</c>
        /// if that header is not present</returns>
        string this[HeaderName header] { get; }

        /// <summary>
        /// Returns the unique identifier associated with the message
        /// </summary>
        MessageId MessageId { get; }

        /// <summary>
        /// Returns the name of the message
        /// </summary>
        /// <remarks>
        /// The message name is used to identify an appropriate type for
        /// content deserialization
        /// </remarks>
        /// <seealso cref="IMessageNamingService"/>
        MessageName MessageName { get; }

        /// <summary>
        /// Returns the date and time at which the message expires
        /// </summary>
        /// <remarks>
        /// Expired messages should not be handled (UTC)
        /// </remarks>
        DateTime Expires { get; }

        /// <summary>
        /// Returns the base URI of the endpoint from which the message
        /// was sent
        /// </summary>
        Uri Origination { get; }
        
        /// <summary>
        /// Returns the base URI of the endpoint to which replies should
        /// be sent (if different than the <see cref="Origination"/>)
        /// </summary>
        Uri ReplyTo { get; }

        /// <summary>
        /// Returns the base URI of the endpoint to which the message
        /// was or is to be sent
        /// </summary>
        Uri Destination { get; }

        /// <summary>
        /// Returns the message ID of the message to which this message
        /// is a reply
        /// </summary>
        MessageId RelatedTo { get; }

        /// <summary>
        /// Returns the date and time the message was published (UTC)
        /// </summary>
        DateTime Published { get; }

        /// <summary>
        /// Returns the name of the topic to which the message was published
        /// </summary>
        TopicName Topic { get; }

        /// <summary>
        /// Returns the date and time the message was sent (UTC)
        /// </summary>
        DateTime Sent { get; }

        /// <summary>
        /// Returns the date and time the message was received (UTC)
        /// </summary>
        DateTime Received { get; }

        /// <summary>
        /// Returns the MIME type of the message content
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Returns the importance of the message
        /// </summary>
        MessageImportance Importance { get; }

        /// <summary>
        /// Returns a security token capturing the claims of the current principal from which the
        /// message was originally received.
        /// </summary>
        string SecurityToken { get; }

        /// <summary>
        /// Returns the specified header value as a URI
        /// </summary>
        /// <param name="headerName">The name of the header</param>
        /// <returns>Returns the specified header value as URI if it exists,
        /// <c>null</c> otherwise</returns>
        Uri GetUri(HeaderName headerName);

        /// <summary>
        /// Returns the specified header value as a UTC date/time
        /// </summary>
        /// <param name="headerName">The name of the header</param>
        /// <returns>Returns the specified header value as UTC date/time if 
        /// it exists, <c>null</c> otherwise</returns>
        DateTime? GetDateTime(HeaderName headerName);
    }
}