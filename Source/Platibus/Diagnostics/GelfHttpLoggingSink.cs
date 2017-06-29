using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Platibus.Http;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// A <see cref="IDiagnosticEventSink"/> that formats events in Graylog Extended Log Format
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
            if (uri == null) throw new ArgumentNullException("uri");
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
        public override Task Process(string gelfMessage)
        {
            return _httpClient.PostAsync(_uri, new StringContent(gelfMessage, Encoding.UTF8, "application/json"));
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
                _httpClient.TryDispose();
            }
        }
    }
}
