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
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.IO
{
    /// <inheritdoc />
    /// <summary>
    /// Formats and writes a <see cref="T:Platibus.Message" /> object to a <see cref="T:System.IO.Stream" /> or 
    /// <see cref="T:System.IO.TextReader" />
    /// </summary>
    public class MessageReader : IDisposable
    {
        private readonly bool _leaveOpen;
        private readonly TextReader _reader;

        private bool _disposed;

        /// <summary>
        /// Initializes a new <see cref="MessageReader"/> that writes formatted messages to
        /// the specified <paramref name="reader"/>
        /// </summary>
        /// <param name="reader">The text reader to which the formatted message will be written</param>
        /// <param name="leaveOpen">(Optional) If <c>false</c>, closes the <see cref="reader"/> when
        /// the <see cref="MessageReader"/> is disposed.  Otherwise, responsibility for closing the
        /// <see cref="reader"/> will be left to the caller.  Defaults to <c>false</c>.</param>
        public MessageReader(TextReader reader, bool leaveOpen = false)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _leaveOpen = leaveOpen;
        }

        /// <summary>
        /// Initializes a new <see cref="MessageReader"/> that writes formatted messages to
        /// the specified <paramref name="stream"/>
        /// </summary>
        /// <param name="stream">The stream to which the formatted message will be written</param>
        /// <param name="encoding">(Optional) The encoding to use.  Defaults to UTF-8.</param>
        /// <param name="leaveOpen">(Optional) If <c>false</c>, closes the <see cref="stream"/> when
        /// the <see cref="MessageReader"/> is disposed.  Otherwise, responsibility for closing the
        /// <see cref="stream"/> will be left to the caller.  Defaults to <c>false</c>.</param>
        public MessageReader(Stream stream, Encoding encoding = null, bool leaveOpen = false)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            _reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
            _leaveOpen = leaveOpen;
        }

        /// <summary>
        /// Reads the message headers from the underlying <see cref="TextReader"/> or <see cref="Stream"/>
        /// </summary>
        /// <returns>The message headers from the underlying reader or stream</returns>
        /// <remarks>
        /// Reads from the stream or reader until the end of the stream or a blank line is encountered.
        /// </remarks>
        public async Task<IMessageHeaders> ReadMessageHeaders()
        {
            var headers = new MessageHeaders();
            var currentHeaderName = (HeaderName) null;
            var currentHeaderValue = new StringWriter();
            var lineNumber = 0;

            string currentLine;
            while (!string.IsNullOrWhiteSpace(currentLine = await _reader.ReadLineAsync()))
            {
                lineNumber++;

                if (currentLine.StartsWith(" ") && currentHeaderName != null)
                {
                    // Continuation of previous header value
                    currentHeaderValue.WriteLine();
                    currentHeaderValue.Write(currentLine.Trim());
                    continue;
                }

                // New header.  Finish up with the header we were just working on.
                if (currentHeaderName != null)
                {
                    headers[currentHeaderName] = currentHeaderValue.ToString();
                    currentHeaderValue = new StringWriter();
                }

                if (currentLine.StartsWith("#"))
                {
                    // Special line. Ignore.
                    continue;
                }

                var separatorPos = currentLine.IndexOf(':');
                if (separatorPos < 0)
                {
                    throw new FormatException($"Invalid header on line {lineNumber}:  Character ':' expected");
                }

                if (separatorPos == 0)
                {
                    throw new FormatException($"Invalid header on line {lineNumber}:  Character ':' found at position 0 (missing header name)");
                }

                currentHeaderName = currentLine.Substring(0, separatorPos);
                currentHeaderValue.Write(currentLine.Substring(separatorPos + 1).Trim());
            }

            if (currentHeaderName != null)
            {
                headers[currentHeaderName] = currentHeaderValue.ToString();
            }
            return headers;
        }

        /// <summary>
        /// Reads the message content underlying <see cref="TextReader"/> or <see cref="Stream"/>
        /// </summary>
        /// <returns>The message headers from the underlying reader or stream</returns>
        /// <remarks>
        /// Reads the remainder of the reader or stream as a string.
        /// </remarks>
        public Task<string> ReadMessageContent()
        {
            return _reader.ReadToEndAsync();
        }

        /// <summary>
        /// Reads the full message from the underlying <see cref="TextReader"/> or <see cref="Stream"/>
        /// </summary>
        /// <returns>The message read from the underlying reader or stream</returns>
        /// <seealso cref="ReadMessageHeaders"/>
        /// <seealso cref="ReadMessageContent"/>
        public async Task<Message> ReadMessage()
        {
            // For backward compatibility with older messages that have
            // a blank line denoting the end of the legacy sender principal...
            if (char.IsWhiteSpace((char) _reader.Peek()))
            {
                // Read the empty blank line so that headers are not
                // mistaken for content
                await _reader.ReadLineAsync();
            }

            var headers = await ReadMessageHeaders();
            var content = await ReadMessageContent();

            return new Message(headers, content);
        }

        /// <inheritdoc />
        ~MessageReader()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes managed resources and releases unmanaged resources
        /// </summary>
        /// <param name="disposing">Whether this method was called from <see cref="Dispose"/></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_reader")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_leaveOpen)
            {
                _reader.Dispose();
            }
        }
    }
}