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
using Platibus.Diagnostics;

namespace Platibus.Multicast
{
    /// <summary>
    /// A diagnostic event related to UDP multicast
    /// </summary>
    public class MulticastEvent : DiagnosticEvent
    {
        /// <summary>
        /// The node to which the multicast event pertains
        /// </summary>
        /// <remarks>
        /// The node to which the event pertains may differ from the node that emitted the event
        /// </remarks>
        public string Node { get; }

        /// <summary>
        /// The host to which the event pertains
        /// </summary>
        /// <remarks>
        /// The host to which the event pertains may differ from the host that emitted the event
        /// </remarks>
        public string Host { get; }

        /// <summary>
        /// The port to which the event pertains
        /// </summary>
        /// <remarks>
        /// The port to which the event pertains may differ from the port of the host that emitted
        /// the event
        /// </remarks>
        public int? Port { get; }

        /// <summary>
        /// Initializes a new <see cref="MulticastEvent"/>
        /// </summary>
        /// <param name="source">The object that emitted the event</param>
        /// <param name="type">The type of event</param>
        /// <param name="detail">Specific details regarding this instance of the event</param>
        /// <param name="exception">The exception related to the event, if applicable</param>
        /// <param name="message">The message to which the event pertains, if applicable</param>
        /// <param name="endpoint">The name of the endpoint, if applicable</param>
        /// <param name="queue">The queue to which the event pertains, if applicable</param>
        /// <param name="topic">The topic to which the message pertains, if applicable</param>
        /// <param name="node">The node to which the multicast event pertains</param>
        /// <param name="host">The node to which the multicast event pertains</param>
        /// <param name="port">The node to which the multicast event pertains</param>
        public MulticastEvent(object source, DiagnosticEventType type, string detail = null, Exception exception = null, Message message = null, EndpointName endpoint = null, QueueName queue = null, TopicName topic = null, string node = null, string host = null, int? port = null) 
            : base(source, type, detail, exception, message, endpoint, queue, topic)
        {
            Node = node;
            Host = host;
            Port = port;
        }
    }
}
