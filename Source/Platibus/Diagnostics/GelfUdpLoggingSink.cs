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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// A <see cref="IDiagnosticService"/> that formats events in Graylog Extended Log Format
    /// (GELF) and posts them to Graylog via its TCP endpoint
    /// </summary>
    public class GelfUdpLoggingSink : GelfLoggingSink
    {
        private const int MaxDatagramLength = 8192;
        private static readonly Random RNG = new Random();
        private static readonly byte[] ChunkedGelfMagicBytes = {0x1e, 0x0f};

        private readonly string _host;
        private readonly int _port;
        private readonly bool _enableCompression;

        /// <summary>
        /// Initializes a new <see cref="GelfUdpLoggingSink"/> that will send GELF formatted
        /// messages to the TCP endpoint at the specified <paramref name="host"/> and
        /// <paramref name="port"/>
        /// </summary>
        /// <param name="host">The hostname of the Graylog TCP endpoint</param>
        /// <param name="port">(Optional) The port of the Gralog TCP endpoint (12201)</param>
        /// <param name="enableCompression">(Optional) Whether to enable compression</param>
        public GelfUdpLoggingSink(string host, int port = 12201, bool enableCompression = false)
        {
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentNullException("host");
            _host = host;
            _port = port;
            _enableCompression = enableCompression;
        }
        
        /// <inheritdoc />
        public override void Process(string gelfMessage)
        {
            var bytes = Encoding.UTF8.GetBytes(gelfMessage);
            if (_enableCompression)
            {
                bytes = Compress(bytes);
            }
            var datagrams = Chunk(bytes);
            using (var udpClient = new UdpClient(_host, _port))
            {
                foreach (var datagram in datagrams)
                {
                    udpClient.Send(datagram, datagram.Length);
                }
            }
        }

        /// <inheritdoc />
        public override async Task ProcessAsync(string gelfMessage, CancellationToken cancellationToken = new CancellationToken())
        {
            var bytes = Encoding.UTF8.GetBytes(gelfMessage);
            if (_enableCompression)
            {
                bytes = Compress(bytes);
            }
            var datagrams = Chunk(bytes);
            using (var udpClient = new UdpClient(_host, _port))
            {
                foreach (var datagram in datagrams)
                {
                    await udpClient.SendAsync(datagram, datagram.Length);
                }
            }
        }

        /// <summary>
        /// Compresses the specified array of <paramref name="bytes"/> using the GZIP algorithm
        /// </summary>
        /// <param name="bytes">The bytes to compress</param>
        /// <returns>Returns the compressed bytes</returns>
        protected virtual byte[] Compress(byte[] bytes)
        {
            var compressByteStream = new MemoryStream();
            var zipStream = new GZipStream(compressByteStream, CompressionMode.Compress);
            try
            {
                zipStream.Write(bytes, 0, bytes.Length);
                zipStream.Flush();
                return compressByteStream.ToArray();
            }
            finally
            {
                zipStream.Close();
            }
        }

        /// <summary>
        /// Breaks a GELF formatted message into sequenced chunks that do not exceed the 
        /// </summary>
        /// <param name="gelfMessageBytes"></param>
        /// <returns></returns>
        protected virtual IEnumerable<byte[]> Chunk(byte[] gelfMessageBytes)
        {
            var messageLength = gelfMessageBytes.Length;
            if (messageLength <= MaxDatagramLength)
            {
                yield return gelfMessageBytes;
                yield break;
            }

            var messageId = NewMessageId();
            var chunkHeader = new byte[12];
            var maxChunkLength = MaxDatagramLength - chunkHeader.Length;
            var sequenceCount = (byte)Math.Ceiling((decimal)gelfMessageBytes.Length / maxChunkLength);
            byte sequenceNumber = 0;

            chunkHeader[0] = ChunkedGelfMagicBytes[0];
            chunkHeader[1] = ChunkedGelfMagicBytes[1];
            Array.Copy(messageId, 0, chunkHeader, 2, 8);
            chunkHeader[10] = sequenceNumber;
            chunkHeader[11] = sequenceCount;
            
            var index = 0;
            while (index < messageLength)
            {
                var chunkLength = messageLength - index;
                if (chunkLength > maxChunkLength)
                {
                    chunkLength = maxChunkLength;
                }
                var chunk = new byte[chunkLength + chunkHeader.Length];
                Array.Copy(chunkHeader, 0, chunk, 0, chunkHeader.Length);
                Array.Copy(gelfMessageBytes, index, chunk, chunkHeader.Length, chunkLength);
                chunk[10] = sequenceNumber;
                sequenceNumber++;
                index += chunkLength;
                yield return chunk;
            }
        }

        /// <summary>
        /// Generates a new message ID to correlate sequences of UDP datagram chunks related to a
        /// single message
        /// </summary>
        /// <returns>Returns an 8-byte message ID</returns>
        protected byte[] NewMessageId()
        {
            var messageId = new byte[8];
            var unixTimeBytes = BitConverter.GetBytes((int)UnixTime.Current.Value);
            var randomBytes = BitConverter.GetBytes(RNG.Next());
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(unixTimeBytes);
                Array.Reverse(randomBytes);
            }
            Array.Copy(unixTimeBytes, 0, messageId, 0, 4);
            Array.Copy(randomBytes, 0, messageId, 4, 4);
            return messageId;
        }
    }
}