// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
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
using Xunit;
using Platibus.Security;

namespace Platibus.UnitTests.Security
{
    [Trait("Category", "UnitTests")]
    public class HexEncodingTests
    {
        [Fact]
        public void OddNumberOfHexDigitsCanBeConvertedToBytes()
        {
            var hex = "A12F5";
            var expected = new byte[] { 10, 18, 245 };
            var actual = HexEncoding.GetBytes(hex);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EvenNumberOfHexDigitsCanBeConvertedToBytes()
        {
            var hex = "0A12F5";
            var expected = new byte[] { 10, 18, 245 };
            var actual = HexEncoding.GetBytes(hex);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LeadingAndTrailingWhiteSpaceIgnored()
        {
            var hex = "  0A12F5\t\r\n";
            var expected = new byte[] { 10, 18, 245 };
            var actual = HexEncoding.GetBytes(hex);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotCaseSensitive()
        {
            var hex = "0a12F5";
            var expected = new byte[] { 10, 18, 245 };
            var actual = HexEncoding.GetBytes(hex);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FormatExceptionThrownForNonHexDigits()
        {
            var hex = "A12G5";
            Assert.Throws<FormatException>(() => HexEncoding.GetBytes(hex));
        }

        [Fact]
        public void HexStringsShouldRoundTrip()
        {
            var hex = "0A12F5";
            var bytes = HexEncoding.GetBytes(hex);
            var hex2 = HexEncoding.GetString(bytes);
            Assert.Equal(hex.ToLower(), hex2.ToLower());
        }

        [Fact]
        public void OddNumberOfHexDigitsShouldRoundTripWithLeadingZero()
        {
            var hex = "A12F5";
            var bytes = HexEncoding.GetBytes(hex);
            var hex2 = HexEncoding.GetString(bytes);
            Assert.Equal("0" + hex.ToLower(), hex2.ToLower());
        }

        [Fact]
        public void ByteArraysShouldRoundTrip()
        {
            var bytes = new byte[] { 10, 18, 245 };
            var hex = HexEncoding.GetString(bytes);
            var bytes2 = HexEncoding.GetBytes(hex);
            Assert.Equal(bytes, bytes2);
        }
    }
}
