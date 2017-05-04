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
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// An object that writes messages to streams
    /// </summary>
    public class MessageWriter : IDisposable
    {
        private readonly bool _leaveOpen;
        private readonly TextWriter _writer;

        private bool _disposed;

        /// <summary>
        /// Initializes a new <see cref="MessageWriter"/> that outputs to the specified
        /// text <paramref name="writer"/>
        /// </summary>
        /// <param name="writer">The text writer to which messages will be written</param>
        /// <param name="leaveOpen">(Optional) Whether to leave the supplied 
        /// <paramref name="writer"/> open when this object is disposed</param>
        /// <remarks>
        /// By default, the <paramref name="writer"/> will be closed when this object is
        /// disposed
        /// </remarks>
        public MessageWriter(TextWriter writer, bool leaveOpen = false)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            _writer = writer;
            _leaveOpen = leaveOpen;
        }

        /// <summary>
        /// Initializes a new <see cref="MessageWriter"/> that outputs to the specified
        /// <paramref name="stream"/>
        /// </summary>
        /// <param name="stream">The stream to which messages will be written</param>
        /// <param name="encoding">(Optional) The encoding to use when converting text to 
        /// bytes</param>
        /// <param name="leaveOpen">(Optional) Whether to leave the supplied 
        /// <paramref name="stream"/> open when this object is disposed</param>
        /// <remarks>
        /// By default, UTF-8 encoding is used and the <paramref name="stream"/> will be 
        /// closed when this object is disposed.
        /// </remarks>
        public MessageWriter(Stream stream, Encoding encoding = null, bool leaveOpen = false)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            _writer = new StreamWriter(stream, encoding ?? Encoding.UTF8);
            _leaveOpen = leaveOpen;
        }
        
        /// <summary>
        /// Writes a message to the underlying stream
        /// </summary>
        /// <param name="message">The message</param>
        /// <returns>Returns a task that completes when the message has been
        /// written to the underlying stream </returns>
        public async Task WriteMessage(Message message)
        {
            if (message == null) throw new ArgumentNullException("message");

            // Write a blank line to preserve backward compatibility with older messages that
            // have a binary formatted SenderPrincipal.  The MessageFileReader will continue to
            // support older messages for the forseeable future to ensure that older messages can
            // still be processed following an update.
            await _writer.WriteLineAsync();

            var headers = message.Headers;
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    var headerName = header.Key;
                    var headerValue = header.Value;
                    await _writer.WriteAsync(string.Format("{0}: ", headerName));
                    using (var headerValueReader = new StringReader(headerValue))
                    {
                        var multilineContinuation = false;
                        string line;
                        while ((line = await headerValueReader.ReadLineAsync()) != null)
                        {
                            if (multilineContinuation)
                            {
                                // Prefix continuation with whitespace so that subsequent
                                // lines are not confused with different headers.
                                line = "    " + line;
                            }
                            await _writer.WriteLineAsync(line);
                            multilineContinuation = true;
                        }
                    }
                }
            }

            // Blank line separates headers from content
            await _writer.WriteLineAsync();

            var content = message.Content;
            if (!string.IsNullOrWhiteSpace(content))
            {
                // No special formatting required for content.  Content is defined as
                // everything following the blank line.
                await _writer.WriteAsync(content);
            }
        }

        /// <summary>
        /// Finalizer that ensures that all resources are released
        /// </summary>
        ~MessageWriter()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called by the <see cref="Dispose()"/> method or finalizer to ensure that
        /// resources are released
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the 
        /// <see cref="Dispose()"/> method (<c>true</c>) or the finalizer (<c>false</c>)</param>
        /// <remarks>
        /// This method will not be called more than once
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_writer")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_leaveOpen)
                {
                    _writer.TryDispose();
                }
            }
        }
    }
}