// The MIT License (MIT)
// 
// Copyright (c) 2015 Jesse Sweetland
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
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Platibus
{
    /// <summary>
    /// Exception thrown to indicate that a connection to a remote bus instance
    /// was refused
    /// </summary>
    [Serializable]
    public class ConnectionRefusedException : TransportException
    {
        private readonly string _host;
        private readonly int _port;

        /// <summary>
        /// Initializes a new <see cref="ConnectionRefusedException"/> with the
        /// specified <paramref name="host"/> and <paramref name="port"/>
        /// </summary>
        /// <param name="host">The host that refused the connection</param>
        /// <param name="port">The port on which the connection was refused</param>
        public ConnectionRefusedException(string host, int port)
            : base(string.Format("Connection refused to {0}:{1}", host, port))
        {
            _host = host;
            _port = port;
        }

        /// <summary>
        /// Initializes a new <see cref="ConnectionRefusedException"/> with the
        /// specified <paramref name="host"/>, <paramref name="port"/>, and nested
        /// exception
        /// </summary>
        /// <param name="host">The host that refused the connection</param>
        /// <param name="port">The port on which the connection was refused</param>
        /// <param name="innerException">The nested exception</param>
        public ConnectionRefusedException(string host, int port, Exception innerException)
            : base(string.Format("Connection refused to {0}:{1}", host, port), innerException)
        {
            _host = host;
            _port = port;
        }

        /// <summary>
        /// Initializes a serialized <see cref="ConnectionRefusedException"/> from a
        /// streaming context
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        public ConnectionRefusedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _host = info.GetString("Host");
            _port = info.GetInt32("Port");
        }

        /// <summary>
        /// The host that refused the connection
        /// </summary>
        public string Host
        {
            get { return _host; }
        }

        /// <summary>
        /// The port on which the connection was refused
        /// </summary>
        public int Port
        {
            get { return _port; }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Host", _host);
            info.AddValue("Port", _port);
        }
    }
}