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

using System.Collections.Generic;

namespace Platibus
{
    /// <summary>
    /// Determines whether two messages are equal
    /// </summary>
    public class MessageEqualityComparer : IEqualityComparer<Message>
    {
        private readonly MessageHeadersEqualityComparer _headersEqualityComparer = new MessageHeadersEqualityComparer();

        /// <summary>
        /// Determines whether two messages are equal
        /// </summary>
        /// <param name="x">One of the messages two compare</param>
        /// <param name="y">The other message to compare</param>
        /// <returns>
        /// Returns <c>true</c> if the two messages are equal; <c>false</c> otherwise
        /// </returns>
        public bool Equals(Message x, Message y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(null, x) || ReferenceEquals(null, y)) return false;

            if (!_headersEqualityComparer.Equals(x.Headers, y.Headers))
            {
                return false;
            }
            return string.Equals(x.Content, y.Content);
        }

        /// <summary>
        /// Returns a hash code for the specified message
        /// </summary>
        /// <param name="message">The message</param>
        /// <returns>Returns a hash code for the specified message</returns>
        public int GetHashCode(Message message)
        {
            if (message == null) return 0;
            var hashCode = _headersEqualityComparer.GetHashCode(message.Headers);
            hashCode = (hashCode*397) ^ (message.Content == null ? 0 : message.Content.GetHashCode());
            return hashCode;
        }
    }
}