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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Platibus.Security;

namespace Platibus.Filesystem
{
    internal class MessageFileReader : IDisposable
    {
        private readonly bool _leaveOpen;
        private readonly TextReader _reader;

        private bool _disposed;

        public MessageFileReader(TextReader reader, bool leaveOpen = false)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _leaveOpen = leaveOpen;
        }

        public MessageFileReader(Stream stream, Encoding encoding = null, bool leaveOpen = false)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            _reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
            _leaveOpen = leaveOpen;
        }

#pragma warning disable 618
        public async Task<IPrincipal> ReadLegacySenderPrincipal()
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
                var senderPrincipal = (SenderPrincipal) formatter.Deserialize(memoryStream);
                return MapToClaimsPrincipal(senderPrincipal);
            }
        }

        private static ClaimsPrincipal MapToClaimsPrincipal(SenderPrincipal senderPrincipal)
        {
            if (senderPrincipal == null) return null;

            var roleClaims = senderPrincipal.Roles.Select(role => new Claim(ClaimTypes.Role, role.Name));
            var claims = new List<Claim>(roleClaims)
            {
                new Claim(ClaimTypes.Name, senderPrincipal.Identity.Name)
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, senderPrincipal.Identity.AuthenticationType));
        }
#pragma warning restore 618

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
                    throw new FormatException($"Invalid header on line {lineNumber}:  Character ':' expected");
                }

                if (separatorPos == 0)
                {
                    throw new FormatException(
                        $"Invalid header on line {lineNumber}:  Character ':' found at position 0 (missing header name)");
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

        ~MessageFileReader()
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
            if (disposing && !_leaveOpen)
            {
                _reader.Dispose();
            }
        }
    }
}