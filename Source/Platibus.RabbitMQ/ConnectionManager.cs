using System;
using System.Collections.Concurrent;
using Common.Logging;
using RabbitMQ.Client;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Maintains a single connection to each endpoint, reconnecting if necessary
    /// </summary>
    class ConnectionManager : IDisposable, IConnectionManager
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
                    DestroyManagedConnection(connection);
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

        private static void DestroyManagedConnection(ManagedConnection connection)
        {
            if (connection == null) return;
            try
            {
                connection.Destroy(true);
            }
            catch (Exception ex)
            {
                Log.Info("Error closing RabbitMQ connection", ex);
            }
        }

    }
}
