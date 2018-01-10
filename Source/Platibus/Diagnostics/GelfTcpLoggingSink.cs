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
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Diagnostics
{
    /// <inheritdoc cref="GelfLoggingSink"/>
    /// <inheritdoc cref="IDisposable"/>
    /// <summary>
    /// A <see cref="T:Platibus.Diagnostics.IDiagnosticService" /> that formats events in Graylog Extended Log Format
    /// (GELF) and posts them to Graylog via its TCP endpoint
    /// </summary>
    public class GelfTcpLoggingSink : GelfLoggingSink, IDisposable
    {
        private readonly object _syncRoot = new object();
        private readonly string _host;
        private readonly int _port;

        private TcpClient _tcpClient;
        private bool _disposed;

        /// <summary>
        /// Initializes a new <see cref="GelfTcpLoggingSink"/> that will send GELF formatted
        /// messages to the TCP endpoint at the specified <paramref name="host"/> and
        /// <paramref name="port"/>
        /// </summary>
        /// <param name="host">The hostname of the Graylog TCP endpoint</param>
        /// <param name="port">(Optional) The port of the Gralog TCP endpoint (12201)</param>
        public GelfTcpLoggingSink(string host, int port = 12201)
        {
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentNullException(nameof(host));
            _host = host;
            _port = port;
        }
        
        /// <inheritdoc />
        public override void Process(string gelfMessage)
        {
            var bytes = Encoding.UTF8.GetBytes(gelfMessage);
            var nullTerminatedBytes = new byte[bytes.Length + 1];
            Array.Copy(bytes, 0, nullTerminatedBytes, 0, bytes.Length);
            nullTerminatedBytes[bytes.Length] = 0;
            Send(nullTerminatedBytes);
        }

        /// <inheritdoc />
        public override Task ProcessAsync(string gelfMessage, CancellationToken cancellationToken = new CancellationToken())
        {
            Process(gelfMessage);
            return Task.FromResult(0);
        }

        private void Send(byte[] nullTerminatedGelfMessageBytes)
        {
            lock (_syncRoot)
            {
                try
                {
                    if (_tcpClient == null || !_tcpClient.Connected)
                    {
                        _tcpClient = new TcpClient(_host, _port);
                    }
                    _tcpClient.GetStream().Write(nullTerminatedGelfMessageBytes, 0, nullTerminatedGelfMessageBytes.Length);
                }
                catch (Exception)
                {
                    try
                    {
                        _tcpClient?.Close();
                    }
                    catch (Exception)
                    {
                        // Ignored
                    }
                    _tcpClient = null;
                    throw;
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
                lock (_tcpClient)
                {
                    if (_tcpClient.Connected)
                    {
                        _tcpClient.Close();
                    }
                }
            }
        }
    }
}