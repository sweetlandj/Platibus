using System;
using System.Collections.Generic;
using System.Net;
using Common.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Maintains an open connection to a RabbitMQ server and attempts to reconnect
    /// whenever there is a failure.
    /// </summary>
    class ManagedConnection : IConnection
    {
        private static readonly ILog Log = LogManager.GetLogger(RabbitMQLoggingCategories.RabbitMQ);

        private readonly object _syncRoot = new object();
        private readonly string _managedConnectionId;
        private readonly IConnectionFactory _connectionFactory;
        private volatile IConnection _connection;

        private bool _destroyed;

        public ManagedConnection(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            var managedConnectionIdBuilder = new UriBuilder(uri)
            {
                // Sanitize credentials
                UserName = "",
                Password = ""
            };
            _managedConnectionId = managedConnectionIdBuilder.Uri.ToString();
            _connectionFactory = new ConnectionFactory {Uri = uri.ToString()};

            Log.InfoFormat("Establishing managed connection to {0}...", _managedConnectionId);
            _connection = _connectionFactory.CreateConnection();
        }

        private IConnection Connection
        {
            get
            {
                CheckDestroyed();
                var myConnection = _connection;
                if (myConnection != null && myConnection.IsOpen) return myConnection;
                lock (_syncRoot)
                {
                    myConnection = _connection;
                    if (myConnection != null && myConnection.IsOpen) return myConnection;

                    Log.InfoFormat("Reconnecting to {0}...", _managedConnectionId);

                    myConnection = _connectionFactory.CreateConnection();
                    myConnection.CallbackException += CallbackException;
                    myConnection.ConnectionShutdown += ConnectionShutdown;
                    myConnection.ConnectionShutdown += (sender, args) => _connection = null;
                    myConnection.ConnectionBlocked += ConnectionBlocked;
                    myConnection.ConnectionUnblocked += ConnectionUnblocked;

                    _connection = myConnection;
                }
                return _connection;
            }
        }

        public IDictionary<string, object> ClientProperties
        {
            get { return Connection.ClientProperties; }
        }

        public EndPoint LocalEndPoint
        {
            get { return Connection.LocalEndPoint; }
        }

        public int LocalPort
        {
            get { return Connection.LocalPort; }
        }

        public EndPoint RemoteEndPoint
        {
            get { return Connection.RemoteEndPoint; }
        }

        public int RemotePort
        {
            get { return Connection.RemotePort; }
        }

        public void Dispose()
        {
        }

        private void CheckDestroyed()
        {
            if (_destroyed) throw new ObjectDisposedException(GetType().FullName);
        }

        ~ManagedConnection()
        {
            Destroy(false);
        }

        public void Destroy(bool disposing)
        {
            if (_destroyed) return;
            _destroyed = true;
            if (disposing)
            {
                Log.InfoFormat("Destroying managed connection to {0}...", _managedConnectionId);
                _connection.TryDispose();
                _connection = null;    
            }
            _destroyed = true;
            GC.SuppressFinalize(this);
        }

        public void Abort()
        {
            if (_connection == null) return;
            lock (_syncRoot)
            {
                if (_connection == null) return;
                _connection.Abort();
                _connection = null;
            }
        }

        public void Abort(ushort reasonCode, string reasonText)
        {
            if (_connection == null) return;
            lock (_syncRoot)
            {
                if (_connection == null) return;
                _connection.Abort(reasonCode, reasonText);
                _connection = null;
            }
        }

        public void Abort(int timeout)
        {
            if (_connection == null) return;
            lock (_syncRoot)
            {
                if (_connection == null) return;
                _connection.Abort(timeout);
                _connection = null;
            }
        }

        public void Abort(ushort reasonCode, string reasonText, int timeout)
        {
            if (_connection == null) return;
            lock (_syncRoot)
            {
                if (_connection == null) return;
                _connection.Abort(reasonCode, reasonText, timeout);
                _connection = null;
            }
        }

        public void Close()
        {
        }

        public void Close(ushort reasonCode, string reasonText)
        {
        }

        public void Close(int timeout)
        {
        }

        public void Close(ushort reasonCode, string reasonText, int timeout)
        {
        }

        public IModel CreateModel()
        {
            return Connection.CreateModel();
        }

        public void HandleConnectionBlocked(string reason)
        {
            Connection.HandleConnectionBlocked(reason);
        }

        public void HandleConnectionUnblocked()
        {
            Connection.HandleConnectionUnblocked();
        }

        public bool AutoClose
        {
            get { return false; }
            set { }
        }

        public ushort ChannelMax
        {
            get { return Connection.ChannelMax; }
        }

        public ShutdownEventArgs CloseReason
        {
            get { return Connection.CloseReason; }
        }

        public AmqpTcpEndpoint Endpoint
        {
            get { return Connection.Endpoint; }
        }

        public uint FrameMax
        {
            get { return Connection.FrameMax; }
        }

        public ushort Heartbeat
        {
            get { return Connection.Heartbeat; }
        }

        public bool IsOpen
        {
            get { return Connection.IsOpen; }
        }

        public AmqpTcpEndpoint[] KnownHosts
        {
            get { return Connection.KnownHosts; }
        }

        public IProtocol Protocol
        {
            get { return Connection.Protocol; }
        }

        public IDictionary<string, object> ServerProperties
        {
            get { return Connection.ServerProperties; }
        }

        public IList<ShutdownReportEntry> ShutdownReport
        {
            get { return Connection.ShutdownReport; }
        }

        public ConsumerWorkService ConsumerWorkService
        {
            get { return Connection.ConsumerWorkService; }
        }

        public event EventHandler<CallbackExceptionEventArgs> CallbackException;
        public event EventHandler<ConnectionBlockedEventArgs> ConnectionBlocked;
        public event EventHandler<ShutdownEventArgs> ConnectionShutdown;
        public event EventHandler<EventArgs> ConnectionUnblocked;

    }
}
