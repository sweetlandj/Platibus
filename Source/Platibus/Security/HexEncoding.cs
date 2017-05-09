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
