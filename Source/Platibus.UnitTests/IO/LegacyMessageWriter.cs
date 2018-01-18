using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.UnitTests.IO
{
    /// <inheritdoc />
    /// <summary>
    /// A copy of the MessageWriter class as it existed prior to the introduction of the
    /// <see cref="T:Platibus.Security.ISecurityTokenService" /> interface and the 
    /// <see cref="P:Platibus.IMessageHeaders.SecurityToken" /> message header.
    /// </summary>
    internal class LegacyMessageWriter : IDisposable
    {
        private readonly bool _leaveOpen;
        private readonly TextWriter _writer;

        private bool _disposed;

        public LegacyMessageWriter(TextWriter writer, bool leaveOpen = false)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _leaveOpen = leaveOpen;
        }

        public LegacyMessageWriter(Stream stream, Encoding encoding = null, bool leaveOpen = false)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            _writer = new StreamWriter(stream, encoding ?? Encoding.UTF8);
            _leaveOpen = leaveOpen;
        }

        public async Task WritePrincipal(IPrincipal principal)
        {
            if (principal != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(memoryStream, principal);
                    var base64String = Convert.ToBase64String(memoryStream.GetBuffer());
                    await _writer.WriteLineAsync(base64String);
                }
            }
            // Blank line to separate subsequent content from Base-64
            // encoded principal data
            await _writer.WriteLineAsync();
        }

        public async Task WriteMessage(Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var headers = message.Headers;
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    var headerName = header.Key;
                    var headerValue = header.Value;
                    await _writer.WriteAsync($"{headerName}: ");
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

        ~LegacyMessageWriter()
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_writer")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_leaveOpen)
            {
                _writer.Dispose();
            }
        }
    }
}
