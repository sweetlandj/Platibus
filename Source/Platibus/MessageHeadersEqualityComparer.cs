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

using System.Collections.Generic;
using System.Linq;

namespace Platibus
{
    /// <summary>
    /// An object that determines whether two groups of message headers are equal
    /// </summary>
    public class MessageHeadersEqualityComparer : IEqualityComparer<IMessageHeaders>
    {
        private readonly IList<HeaderName> _ignoredHeaders;

        /// <summary>
        /// Initializes a new <see cref="MessageHeadersEqualityComparer"/>
        /// </summary>
        /// <param name="ignoredHeaders">(Optional) The headers to ignore</param>
        public MessageHeadersEqualityComparer(params HeaderName[] ignoredHeaders)
        {
            _ignoredHeaders = ignoredHeaders == null ? new List<HeaderName>() : ignoredHeaders.ToList();
        }

        /// <summary>
        /// Initializes a new <see cref="MessageHeadersEqualityComparer"/>
        /// </summary>
        /// <param name="ignoredHeaders">(Optional) The headers to ignore</param>
        public MessageHeadersEqualityComparer(IEnumerable<HeaderName> ignoredHeaders)
        {
            _ignoredHeaders = ignoredHeaders == null ? new List<HeaderName>() : ignoredHeaders.ToList();
        }

        /// <summary>
        /// Determines whether two sets of message headers are equal by comparing
        /// their header names and values
        /// </summary>
        /// <param name="x">The first set of headers to compare</param>
        /// <param name="y">The other set of headers to compare</param>
        /// <returns>Returns <c>true</c> if both sets of headers are equal; <c>false</c>
        /// otherwise</returns>
        /// <remarks>
        /// Two sets of message headers are equal if they contain exactly the same
        /// header names with exactly the same values
        /// </remarks>
        public bool Equals(IMessageHeaders x, IMessageHeaders y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(null, x) || ReferenceEquals(null, y)) return false;

            var xs = x.Where(xel => !_ignoredHeaders.Contains(xel.Key)).ToList();
            var ys = y.Where(xel => !_ignoredHeaders.Contains(xel.Key)).ToList();

            return xs.Count == ys.Count && xs.All(xel => Equals(xel.Value, y[xel.Key]));
        }

        /// <summary>
        /// Returns a hash code for a set of message headers
        /// </summary>
        /// <param name="headers">The message headers</param>
        /// <returns>Returns a hash code for the specified message headers</returns>
        public int GetHashCode(IMessageHeaders headers)
        {
            if (headers == null) return 0;
            var xs = headers.Where(xel => !_ignoredHeaders.Contains(xel.Key)).ToList();
            return xs.Aggregate(0, (current, entry) => (current*397) ^ entry.Key.GetHashCode());
        }
        
    }
}