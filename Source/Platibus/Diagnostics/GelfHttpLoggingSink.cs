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
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Http;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// A <see cref="IDiagnosticService"/> that formats events in Graylog Extended Log Format
    /// (GELF) and posts them to Graylog via its HTTP REST API
    /// </summary>
    public class GelfHttpLoggingSink : GelfLoggingSink, IDisposable
    {
        private readonly Uri _uri;
        private readonly HttpClient _httpClient;

        private bool _disposed;

        /// <summary>
        /// Initializes a new <see cref="GelfHttpLoggingSink"/> that will POST GELF formatted
        /// messages to the specified <paramref name="uri"/>
        /// </summary>
        /// <param name="uri">The URI to which the GELF formatted messages should be POSTed</param>
        /// <param name="credentials">(Optional) Credentials to use to authenticate with the
        /// Graylog server if needed</param>
        public GelfHttpLoggingSink(Uri uri, IEndpointCredentials credentials = null)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            _uri = uri;
            var httpClientHandler = new HttpClientHandler();
            _httpClient = new HttpClient(httpClientHandler, true);
            if (credentials != null)
            {
                var visitor = new HttpEndpointCredentialsVisitor(httpClientHandler, _httpClient);
                credentials.Accept(visitor);
            }
        }

        /// <inheritdoc />
        public override void Process(string gelfMessage)
        {
            _httpClient.PostAsync(_uri, new StringContent(gelfMessage, Encoding.UTF8, "application/json")).Wait();
        }

        /// <inheritdoc />
        public override Task ProcessAsync(string gelfMessage, CancellationToken cancellationToken = default(CancellationToken))
        {
            var content = new StringContent(gelfMessage, Encoding.UTF8, "application/json");
            return _httpClient.PostAsync(_uri, content, cancellationToken);
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
        /// Disposes managed resources
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the
        /// <see cref="Dispose()"/> method
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient.Dispose();
            }
        }
    }
}
