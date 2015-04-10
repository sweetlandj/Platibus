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

using System.Collections.Generic;

namespace Platibus
{
    public class MessageEqualityComparer : IEqualityComparer<Message>
    {
        private readonly MessageHeadersEqualityComparer _headersEqualityComparer = new MessageHeadersEqualityComparer();

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

        public int GetHashCode(Message obj)
        {
            if (obj == null) return 0;
            var hashCode = _headersEqualityComparer.GetHashCode(obj.Headers);
            hashCode = (hashCode*397) ^ (obj.Content == null ? 0 : obj.Content.GetHashCode());
            return hashCode;
        }
    }
}