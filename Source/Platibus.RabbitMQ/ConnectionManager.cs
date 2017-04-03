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
using System.Collections.Concurrent;
using Common.Logging;
using RabbitMQ.Client;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Maintains a single connection to each endpoint, reconnecting if necessary
    /// </summary>
    internal class ConnectionManager : IDisposable, IConnectionManager
    {
        private static readonly ILog Log = LogManager.GetLogger(RabbitMQLoggingCategories.RabbitMQ);
        private readonly ConcurrentDictionary<Uri, ManagedConnection> _managedConnections = new ConcurrentDictionary<Uri, ManagedConnection>();
        private volatile bool _disposed;

        /// <summary>
        /// Returns a RabbitMQ connection to the server at the specified
        /// <paramref name="uri"/>
        /// </summary>
        /// <param name="uri">The URI of the RabbitMQ server</param>
        /// <returns>Returns a connection to the specified RabbitMQ server</returns>
        public IConnection GetConnection(Uri uri)
        {
            CheckDisposed();
            if (uri == null) throw new ArgumentNullException("uri");
            uri = uri.WithoutTrailingSlash();
            return _managedConnections.GetOrAdd(uri, CreateManagedConnection);
        }

        /// <summary>
        /// Finalizer that ensures all resources are released
        /// </summary>
        ~ConnectionManager()
        {
            if (_disposed) return;
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (_disposed) return;
            // Set _disposed to true before actually disposing to prevent new connections 
            // from being created during disposal.  I thought about creating a separate 
            // _disposing indicator for this, but there would never be a situation in which
            // one variable is checked without also checking the other.
            _disposed = true;
            Dispose(true);
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
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var uri in _managedConnections.Keys)
                {
                    ManagedConnection connection;
                    _managedConnections.TryRemove(uri, out connection);
                    CloseManagedConnection(connection);
                }
            }
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        private static ManagedConnection CreateManagedConnection(Uri uri)
        {
            return new ManagedConnection(uri);
        }

        private static void CloseManagedConnection(ManagedConnection connection)
        {
            if (connection == null) return;
            try
            {
                connection.Close();
                return;
            }
            catch (Exception ex)
            {
                Log.Info("Error closing RabbitMQ connection.  Attempting to destroy...", ex);
            }

            try
            {
                connection.Destroy(true);
            }
            catch (Exception ex)
            {
                Log.Info("Error destroying RabbitMQ connection", ex);
            }
        }

    }
}
