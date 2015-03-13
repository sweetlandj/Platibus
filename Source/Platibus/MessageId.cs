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
    public struct MessageId : IEquatable<MessageId>
    {
        private readonly Guid _value;

        public MessageId(Guid value)
        {
            _value = value;
        }

        public bool Equals(MessageId messageId)
        {
            return _value == messageId._value;
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public override bool Equals(object obj)
        {
            return (obj is MessageId) && Equals((MessageId) obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static bool operator ==(MessageId left, MessageId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MessageId left, MessageId right)
        {
            return !Equals(left, right);
        }

        public static implicit operator MessageId(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? new MessageId() : new MessageId(Guid.Parse(value));
        }

        public static implicit operator string(MessageId messageId)
        {
            return messageId._value.ToString();
        }

        public static implicit operator MessageId(Guid value)
        {
            return new MessageId(value);
        }

        public static implicit operator Guid(MessageId messageId)
        {
            return messageId == null ? Guid.Empty : messageId._value;
        }

        public static MessageId Generate()
        {
            return new MessageId(Guid.NewGuid());
        }
    }
}