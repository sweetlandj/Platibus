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

using System;

namespace Platibus.IntegrationTests
{
    public class TestPublication : IEquatable<TestPublication>
    {
        public Guid GuidData { get; set; }
        public int IntData { get; set; }
        public string StringData { get; set; }
        public DateTime DateData { get; set; }

        public bool Equals(TestPublication other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return GuidData.Equals(other.GuidData) &&
                   IntData == other.IntData &&
                   string.Equals(StringData, other.StringData) &&
                   DateData.Equals(other.DateData);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((TestPublication) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = GuidData.GetHashCode();
                hashCode = (hashCode * 397) ^ IntData;
                hashCode = (hashCode * 397) ^ (StringData != null ? StringData.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ DateData.GetHashCode();
                return hashCode;
            }
        }
    }
}