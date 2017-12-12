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

using Platibus.Diagnostics;

namespace Platibus.Multicast
{
    /// <summary>
    /// Types of <see cref="DiagnosticEvent"/>s specific to UDP multicast
    /// </summary>
    public class MulticastEventType
    {
        /// <summary>
        /// Emitted when a broadcast socket is bound
        /// </summary>
        public static readonly DiagnosticEventType BroadcastSocketBound = new DiagnosticEventType("BroadcastSocketBound", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted when a listener socket is bound
        /// </summary>
        public static readonly DiagnosticEventType ListenerSocketBound = new DiagnosticEventType("ListenerSocketBound", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted when a listener is started
        /// </summary>
        public static readonly DiagnosticEventType ListenerStarted = new DiagnosticEventType("ListenerStarted", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted when a listener is stopped
        /// </summary>
        public static readonly DiagnosticEventType ListenerStopped = new DiagnosticEventType("ListenerStopped", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted when a datagram is received
        /// </summary>
        public static readonly DiagnosticEventType DatagramReceived = new DiagnosticEventType("DatagramReceived", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted when a datagram is broadcast to the multicast group
        /// </summary>
        public static readonly DiagnosticEventType DatagramBroadcast = new DiagnosticEventType("DatagramBroadcast", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted when there is an error broadcasting a datagram to the multicast group
        /// </summary>
        public static readonly DiagnosticEventType DatagramBroadcastError = new DiagnosticEventType("DatagramBroadcastError", DiagnosticEventLevel.Warn);
        
        /// <summary>
        /// Emitted when a the received datagram is not correctly formatted
        /// </summary>
        public static readonly DiagnosticEventType MalformedDatagram = new DiagnosticEventType("MalformedDatagram", DiagnosticEventLevel.Trace);
    }
}
