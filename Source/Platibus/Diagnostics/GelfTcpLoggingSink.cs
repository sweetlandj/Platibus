using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// A <see cref="IDiagnosticEventSink"/> that formats events in Graylog Extended Log Format
    /// (GELF) and posts them to Graylog via its TCP endpoint
    /// </summary>
    public class GelfTcpLoggingSink : GelfLoggingSink, IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private readonly TcpClient _tcpClient;
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
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentNullException("host");
            _tcpClient = new TcpClient();
            _host = host;
            _port = port;
        }
        
        /// <inheritdoc />
        public override Task Process(string gelfMessage)
        {
            var bytes = Encoding.ASCII.GetBytes(gelfMessage);
            var nullTerminatedBytes = new byte[bytes.Length + 1];
            Array.Copy(bytes, 0, nullTerminatedBytes, 0, bytes.Length);
            nullTerminatedBytes[bytes.Length] = 0;
            Send(nullTerminatedBytes);
            return Task.FromResult(0);
        }

        private void Send(byte[] nullTerminatedGelfMessageBytes)
        {
            lock (_tcpClient)
            {
                try
                {
                    if (!_tcpClient.Connected)
                    {
                        _tcpClient.Connect(_host, _port);
                    }
                    _tcpClient.GetStream().Write(nullTerminatedGelfMessageBytes, 0, nullTerminatedGelfMessageBytes.Length);
                }
                catch (Exception)
                {
                    try
                    {
                        _tcpClient.Close();
                    }
                    catch (Exception)
                    {
                    }
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