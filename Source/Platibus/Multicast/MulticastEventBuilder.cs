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
    /// Mutable factory object that produces immutable diagnostic events[
    /// </summary>
    public class MulticastEventBuilder : DiagnosticEventBuilder
    {
        /// <summary>
        /// The node to which the multicast event pertains
        /// </summary>
        /// <remarks>
        /// The node to which the event pertains may differ from the node that emitted the event
        /// </remarks>
        public string Node { get; set; }

        /// <summary>
        /// The host to which the event pertains
        /// </summary>
        /// <remarks>
        /// The host to which the event pertains may differ from the host that emitted the event
        /// </remarks>
        public string Host { get; set; }

        /// <summary>
        /// The port to which the event pertains
        /// </summary>
        /// <remarks>
        /// The port to which the event pertains may differ from the port of the host that emitted
        /// the event
        /// </remarks>
        public int? Port { get; set; }

        /// <summary>
        /// Initializes a new <see cref="DiagnosticEventBuilder"/> for the specified 
        /// </summary>
        /// <param name="source">The object that will produce the event</param>
        /// <param name="type">The type of event</param>
        public MulticastEventBuilder(object source, DiagnosticEventType type) : base(source, type)
        {
        }

        /// <inheritdoc />
        public override DiagnosticEvent Build()
        {
            return new MulticastEvent(Source, Type, Detail, Exception, Message, Endpoint, Queue, Topic, Node, Host, Port);
        }
    }
}
