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
using Platibus.Diagnostics;
using Platibus.Utils;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Maintains an open connection to a RabbitMQ server and attempts to reconnect
    /// whenever there is a failure.
    /// </summary>
    internal class ManagedConnection : IConnection
    {
        private readonly object _syncRoot = new object();
        private readonly IDiagnosticService _diagnosticService;
        private readonly IConnection _connection;

        private bool _closed;

        public string ManagedConnectionId { get; }

        public ManagedConnection(Uri uri, IDiagnosticService diagnosticService)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            // Trailing slashes causes errors when connecting to RabbitMQ
            uri = uri.WithoutTrailingSlash();

            var managedConnectionIdBuilder = new UriBuilder(uri)
            {
                // Sanitize credentials in managed connection ID.  This value
                // is output in log messages for diagnostics, so we don't want
                // or need the credentials displayed.
                UserName = "",
                Password = ""
            };

            ManagedConnectionId = managedConnectionIdBuilder.Uri.ToString();
            IConnectionFactory connectionFactory = new ConnectionFactory
            {
                Uri = uri,
                AutomaticRecoveryEnabled = true,
                RequestedHeartbeat = 10,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };
            _diagnosticService = diagnosticService ?? throw new ArgumentNullException(nameof(diagnosticService));
            _connection = connectionFactory.CreateConnection();

            _diagnosticService.Emit(new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQConnectionOpened)
            {
                Detail = "Opened managed connection ID " + ManagedConnectionId
            }.Build());
        }
        
        public string ClientProvidedName => _connection.ClientProvidedName;

        public IDictionary<string, object> ClientProperties => _connection.ClientProperties;

        public int LocalPort => _connection.LocalPort;

        public int RemotePort => _connection.RemotePort;

        public void Dispose()
        {
        }
        
        ~ManagedConnection()
        {
            CloseManagedConnection(false);
        }

        public void CloseManagedConnection(bool disposing)
        {
            if (_closed) return;

            _closed = true;
            if (disposing)
            {
                lock (_syncRoot)
                {
                    _connection.Close();
                }
                
                _diagnosticService.Emit(new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQConnectionClosed)
                {
                    Detail = "Managed connection ID " + ManagedConnectionId + " successfully closed"
                }.Build());
            }
            GC.SuppressFinalize(this);
        }

        public void Abort()
        {
            if (_connection == null) return;
            lock (_syncRoot)
            {
                if (_connection == null) return;
                _connection.Abort();
                _diagnosticService.Emit(new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQConnectionAborted)
                {
                    Detail = "Managed connection ID " + ManagedConnectionId + " aborted"
                }.Build());
            }
        }

        public void Abort(ushort reasonCode, string reasonText)
        {
            if (_connection == null) return;
            lock (_syncRoot)
            {
                if (_connection == null) return;
                _connection.Abort(reasonCode, reasonText);
            }
        }

        public void Abort(int timeout)
        {
            if (_connection == null) return;
            lock (_syncRoot)
            {
                if (_connection == null) return;
                _connection.Abort(timeout);
            }
        }

        public void Abort(ushort reasonCode, string reasonText, int timeout)
        {
            if (_connection == null) return;
            lock (_syncRoot)
            {
                if (_connection == null) return;
                _connection.Abort(reasonCode, reasonText, timeout);
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
            return _connection.CreateModel();
        }

        public void HandleConnectionBlocked(string reason)
        {
            _connection.HandleConnectionBlocked(reason);
        }

        public void HandleConnectionUnblocked()
        {
            _connection.HandleConnectionUnblocked();
        }

        public bool AutoClose
        {
            get { return false; }
            set { }
        }

        public ushort ChannelMax => _connection.ChannelMax;

        public ShutdownEventArgs CloseReason => _connection.CloseReason;

        public AmqpTcpEndpoint Endpoint => _connection.Endpoint;

        public uint FrameMax => _connection.FrameMax;

        public ushort Heartbeat => _connection.Heartbeat;

        public bool IsOpen => _connection.IsOpen;

        public AmqpTcpEndpoint[] KnownHosts => _connection.KnownHosts;

        public IProtocol Protocol => _connection.Protocol;

        public IDictionary<string, object> ServerProperties => _connection.ServerProperties;

        public IList<ShutdownReportEntry> ShutdownReport => _connection.ShutdownReport;

        public ConsumerWorkService ConsumerWorkService => _connection.ConsumerWorkService;

        public event EventHandler<CallbackExceptionEventArgs> CallbackException
        {
            add => _connection.CallbackException += value;
            remove => _connection.CallbackException -= value;
        }

        public event EventHandler<ConnectionBlockedEventArgs> ConnectionBlocked
        {
            add => _connection.ConnectionBlocked += value;
            remove => _connection.ConnectionBlocked -= value;
        }

        public event EventHandler<ShutdownEventArgs> ConnectionShutdown
        {
            add => _connection.ConnectionShutdown += value;
            remove => _connection.ConnectionShutdown -= value;
        }

        public event EventHandler<EventArgs> ConnectionUnblocked
        {
            add => _connection.ConnectionUnblocked += value;
            remove => _connection.ConnectionUnblocked -= value;
        }

        public event EventHandler<EventArgs> RecoverySucceeded
        {
            add => _connection.RecoverySucceeded += value;
            remove => _connection.RecoverySucceeded -= value;
        }

        public event EventHandler<ConnectionRecoveryErrorEventArgs> ConnectionRecoveryError
        {
            add => _connection.ConnectionRecoveryError += value;
            remove => _connection.ConnectionRecoveryError -= value;
        }
    }
}
