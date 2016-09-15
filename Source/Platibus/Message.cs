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

namespace Platibus
{
    /// <summary>
    /// Represents a message with its headers and serialized content
    /// </summary>
    public class Message
    {
        private readonly string _content;
        private readonly IMessageHeaders _headers;

        /// <summary>
        /// Initializes a new <see cref="Message"/> with the specified <paramref name="headers"/>
        /// and <paramref name="content"/>
        /// </summary>
        /// <param name="headers">The message headers</param>
        /// <param name="content">The serialized message content</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="headers"/> is
        /// <c>null</c></exception>
        public Message(IMessageHeaders headers, string content)
        {
            if (headers == null) throw new ArgumentNullException("headers");
            _headers = headers;
            _content = content ?? "";
        }

        /// <summary>
        /// The message headers
        /// </summary>
        public IMessageHeaders Headers
        {
            get { return _headers; }
        }

        /// <summary>
        /// The serialized message content
        /// </summary>
        public string Content
        {
            get { return _content; }
        }
    }
}