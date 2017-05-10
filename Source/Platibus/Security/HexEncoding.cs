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

namespace Platibus.Security
{
    /// <summary>
    /// Helper class for encoding and decoding byte arrays into their hexadecimal string 
    /// representations
    /// </summary>
    public class HexEncoding
    {
        private const string HexDigits = "0123456789abcdef";

        /// <summary>
        /// Converts a hexadecimal string into a byte array
        /// </summary>
        /// <param name="hex">The hexadecimal string</param>
        /// <returns>Returns the byte array represented by the specified <paramref name="hex"/>
        /// encoded string</returns>
        public static byte[] GetBytes(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) throw new ArgumentNullException("hex");

            var trimmedHex = hex.Trim().ToLower();
            var len = trimmedHex.Length;
            if ((len & 1) != 0)
            {
                // String contains an odd number of characters (nibbles) so pad the leftmost/most
                // significant nibble with a leading zero
                trimmedHex = '0' + trimmedHex;
                len++;
            }

            var byteLen = len >> 1;
            var bytes = new byte[byteLen];
            for (var i = 0; i < byteLen; i++)
            {
                var off = i << 1;
                var msn = HexDigits.IndexOf(trimmedHex[off]);
                if (msn < 0) throw new FormatException("Invalid hexadecimal digit '" + trimmedHex[off] + "' at position " + off);

                var lsn = HexDigits.IndexOf(trimmedHex[off + 1]);
                if (lsn < 0) throw new FormatException("Invalid hexadecimal digit '" + trimmedHex[off + 1] + "' at position " + (off + 1));

                bytes[i] = (byte)(((msn << 4) & 0xF0) | (lsn & 0x0F));
            }

            return bytes;
        }

        /// <summary>
        /// Converts a byte array into a hexadecimal string
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>Returns the hexadecimal string representation of the specified
        /// <paramref name="bytes"/> encoded string</returns>
        public static string GetString(byte[] bytes)
        {
            if (bytes == null) return null;

            var byteLen = bytes.Length;
            var buf = new char[byteLen << 1];
            for (var i = 0; i < byteLen; i++)
            {
                var b = bytes[i];
                var off = i << 1;
                var msn = (b & 0xF0) >> 4;
                buf[off] = HexDigits[msn];
                var lsn = b & 0x0F;
                buf[off + 1] = HexDigits[lsn];
            }
            return new string(buf);
        }
    }
}
