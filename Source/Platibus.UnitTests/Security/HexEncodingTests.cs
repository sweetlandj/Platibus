using System;
using NUnit.Framework;
using Platibus.Security;

namespace Platibus.UnitTests.Security
{
    public class HexEncodingTests
    {
        [Test]
        public void OddNumberOfHexDigitsCanBeConvertedToBytes()
        {
            var hex = "A12F5";
            var expected = new byte[] { 10, 18, 245 };
            var actual = HexEncoding.GetBytes(hex);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void EvenNumberOfHexDigitsCanBeConvertedToBytes()
        {
            var hex = "0A12F5";
            var expected = new byte[] { 10, 18, 245 };
            var actual = HexEncoding.GetBytes(hex);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void LeadingAndTrailingWhiteSpaceIgnored()
        {
            var hex = "  0A12F5\t\r\n";
            var expected = new byte[] { 10, 18, 245 };
            var actual = HexEncoding.GetBytes(hex);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NotCaseSensitive()
        {
            var hex = "0a12F5";
            var expected = new byte[] { 10, 18, 245 };
            var actual = HexEncoding.GetBytes(hex);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FormatExceptionThrownForNonHexDigits()
        {
            var hex = "A12G5";
            Assert.Throws<FormatException>(() => HexEncoding.GetBytes(hex));
        }

        [Test]
        public void HexStringsShouldRoundTrip()
        {
            var hex = "0A12F5";
            var bytes = HexEncoding.GetBytes(hex);
            var hex2 = HexEncoding.GetString(bytes);
            Assert.AreEqual(hex.ToLower(), hex2.ToLower());
        }

        [Test]
        public void OddNumberOfHexDigitsShouldRoundTripWithLeadingZero()
        {
            var hex = "A12F5";
            var bytes = HexEncoding.GetBytes(hex);
            var hex2 = HexEncoding.GetString(bytes);
            Assert.AreEqual("0" + hex.ToLower(), hex2.ToLower());
        }

        [Test]
        public void ByteArraysShouldRoundTrip()
        {
            var bytes = new byte[] { 10, 18, 245 };
            var hex = HexEncoding.GetString(bytes);
            var bytes2 = HexEncoding.GetBytes(hex);
            Assert.AreEqual(bytes, bytes2);
        }
    }
}
