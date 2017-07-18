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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Http
{
    /// <summary>
    /// A collection of long-lived HTTP clients pooled according to endpoint URI and credentials
    /// </summary>
    public class HttpClientPool : IDisposable
    {
        private readonly SemaphoreSlim _poolSync = new SemaphoreSlim(1);
        private readonly IDictionary<PoolKey, HttpClientHandler> _pool = new Dictionary<PoolKey, HttpClientHandler>();

        private bool _disposed;

        /// <summary>
        /// Indicates the number of HTTP clients currently in the pool
        /// </summary>
        public int Size
        {
            get { return _pool.Count; }
        }

        /// <summary>
        /// Gets an HTTP client from the pool, creating a new HTTP client if necessary
        /// </summary>
        /// <param name="uri">The URI for the connection</param>
        /// <param name="credentials">(Optional) The credentials needed to make the connection</param>
        /// <param name="cancellationToken">(Optional) A token the can be used byy the caller
        /// to request cancellation of the aquisition attempt</param>
        /// <returns>
        /// Returns a task whose result is an HTTP client that can be used to connect to the
        /// specified <paramref name="uri"/> with the supplied <paramref name="credentials"/>
        /// </returns>
        public async Task<HttpClient> GetClient(Uri uri, IEndpointCredentials credentials, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            var newHandler = false;
            var key = new PoolKey(uri, credentials);
            HttpClientHandler clientHandler;
            if (_pool.TryGetValue(key, out clientHandler))
            {
                return CreateClient(clientHandler, uri, credentials, newHandler);
            }

            await _poolSync.WaitAsync(cancellationToken);
            try
            {
                if (!_pool.TryGetValue(key, out clientHandler))
                {
                    newHandler = true;
                    clientHandler = new HttpClientHandler
                    {
                        AllowAutoRedirect = true,
                        UseProxy = false
                    };
                    
                    _pool[key] = clientHandler;

                    // Make sure DNS TTL is honored
                    var sp = ServicePointManager.FindServicePoint(uri);
                    sp.ConnectionLeaseTimeout = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;
                }
            }
            finally
            {
                _poolSync.Release();
            }

            return CreateClient(clientHandler, uri, credentials, newHandler);
        }

        private static HttpClient CreateClient(HttpClientHandler clientHandler, Uri baseAddress, IEndpointCredentials credentials, bool newHandler)
        {
            var client = new HttpClient(clientHandler, false)
            {
                BaseAddress = baseAddress
            };

            if (credentials != null)
            {
                credentials.Accept(new HttpEndpointCredentialsVisitor(clientHandler, client, newHandler));
            }
            return client;
        }

        /// <summary>
        /// Throws an exception if this object has already been disposed
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object has been disposed</exception>
        protected void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        /// <summary>
        /// Disposes the pool and any open HTTP clients
        /// </summary>
        /// <param name="disposing">
        /// Whether this method was called from <see cref="Dispose()"/>
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_poolSync")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _poolSync.TryDispose();
                foreach (var httpClientHandler in _pool)
                {
                    httpClientHandler.Value.TryDispose();
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }
        
        private class PoolKey : IEquatable<PoolKey>
        {
            private readonly Uri _uri;
            private readonly Type _credentialType;

            public PoolKey(Uri uri, IEndpointCredentials credentials)
            {
                _uri = uri;
                _credentialType = credentials == null ? null : credentials.GetType();
            }

            public bool Equals(PoolKey other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(_uri, other._uri) && _credentialType == other._credentialType;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((PoolKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((_uri != null ? _uri.GetHashCode() : 0) * 397) ^ (_credentialType != null ? _credentialType.GetHashCode() : 0);
                }
            }

            public static bool operator ==(PoolKey left, PoolKey right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(PoolKey left, PoolKey right)
            {
                return !Equals(left, right);
            }
        }
    }
}
