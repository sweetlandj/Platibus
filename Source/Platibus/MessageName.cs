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
    public class MessageName : IEquatable<MessageName>
    {
        private readonly string _value;

        public MessageName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("value");
            _value = value.Trim();
        }

        public bool Equals(MessageName messageName)
        {
            if (ReferenceEquals(null, messageName)) return false;
            return string.Equals(_value, messageName._value, StringComparison.InvariantCultureIgnoreCase);
        }

        public override string ToString()
        {
            return _value;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MessageName);
        }

        public override int GetHashCode()
        {
            return _value.ToLowerInvariant().GetHashCode();
        }

        public static bool operator ==(MessageName left, MessageName right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MessageName left, MessageName right)
        {
            return !Equals(left, right);
        }

        public static implicit operator MessageName(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : new MessageName(value);
        }

        public static implicit operator string(MessageName messageName)
        {
            return messageName == null ? null : messageName._value;
        }
    }
}