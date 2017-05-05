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
    internal class HttpClientPool : IDisposable
    {
        private readonly SemaphoreSlim _poolSync = new SemaphoreSlim(1);
        private readonly IDictionary<PoolKey, HttpClientHandler> _pool = new Dictionary<PoolKey, HttpClientHandler>();

        private bool _disposed;

        public int Size
        {
            get { return _pool.Count; }
        }

        public async Task<HttpClient> GetClient(Uri uri, IEndpointCredentials credentials, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            var key = new PoolKey(uri, credentials);
            HttpClientHandler clientHandler;
            if (_pool.TryGetValue(key, out clientHandler))
            {
                return CreateClient(clientHandler, uri, credentials);
            }

            await _poolSync.WaitAsync(cancellationToken);
            try
            {
                if (!_pool.TryGetValue(key, out clientHandler))
                {
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

            return CreateClient(clientHandler, uri, credentials);
        }

        private static HttpClient CreateClient(HttpClientHandler clientHandler, Uri baseAddress, IEndpointCredentials credentials)
        {
            var client = new HttpClient(clientHandler, false)
            {
                BaseAddress = baseAddress
            };

            if (credentials != null)
            {
                credentials.Accept(new HttpEndpointCredentialsVisitor(clientHandler, client));
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
