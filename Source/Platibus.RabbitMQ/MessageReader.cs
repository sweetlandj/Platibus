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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.RabbitMQ
{
    internal class MessageReader : IDisposable
    {
        private readonly bool _leaveOpen;
        private readonly TextReader _reader;

        private bool _disposed;

        public MessageReader(TextReader reader, bool leaveOpen = false)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            _reader = reader;
            _leaveOpen = leaveOpen;
        }

        public MessageReader(Stream stream, Encoding encoding = null, bool leaveOpen = false)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            _reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
            _leaveOpen = leaveOpen;
        }

        public async Task<IPrincipal> ReadPrincipal()
        {
            var base64Buffer = new StringBuilder();
            string currentLine;
            while (!string.IsNullOrWhiteSpace(currentLine = await _reader.ReadLineAsync()))
            {
                base64Buffer.AppendLine(currentLine);
            }

            var base64String = base64Buffer.ToString();

            if (string.IsNullOrWhiteSpace(base64String)) return null;

            var bytes = Convert.FromBase64String(base64String);
            using (var memoryStream = new MemoryStream(bytes))
            {
                var formatter = new BinaryFormatter();
                return (IPrincipal) formatter.Deserialize(memoryStream);
            }
        }

        public async Task<Message> ReadMessage()
        {
            var headers = new MessageHeaders();
            var currentHeaderName = (HeaderName) null;
            var currentHeaderValue = new StringWriter();
            var finishedReadingHeaders = false;
            var lineNumber = 0;

            string currentLine;
            while (!finishedReadingHeaders && (currentLine = await _reader.ReadLineAsync()) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(currentLine))
                {
                    if (currentHeaderName != null)
                    {
                        headers[currentHeaderName] = currentHeaderValue.ToString();
                        currentHeaderName = null;
                        currentHeaderValue = new StringWriter();
                    }

                    finishedReadingHeaders = true;
                    continue;
                }

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
                    throw new FormatException(string.Format("Invalid header on line {0}:  Character ':' expected",
                        lineNumber));
                }

                if (separatorPos == 0)
                {
                    throw new FormatException(
                        string.Format(
                            "Invalid header on line {0}:  Character ':' found at position 0 (missing header name)",
                            lineNumber));
                }

                currentHeaderName = currentLine.Substring(0, separatorPos);
                currentHeaderValue.Write(currentLine.Substring(separatorPos + 1).Trim());
            }

            var content = "";
            if (finishedReadingHeaders)
            {
                content = await _reader.ReadToEndAsync();
            }
            return new Message(headers, content);
        }

        ~MessageReader()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_leaveOpen)
                {
                    _reader.Dispose();
                }
            }
        }
    }
}