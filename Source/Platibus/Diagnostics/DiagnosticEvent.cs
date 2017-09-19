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

namespace Platibus.Diagnostics
{
    /// <summary>
    /// A diagnostic event
    /// </summary>
    public class DiagnosticEvent
    {
        private readonly object _source;
        private readonly DateTime _timestamp;
        private readonly DiagnosticEventType _type;
        private readonly string _detail;
        private readonly Exception _exception;
        private readonly Message _message;
        private readonly EndpointName _endpoint;
        private readonly QueueName _queue;
        private readonly TopicName _topic;

        /// <summary>
        /// The object that emitted the event
        /// </summary>
        public object Source { get { return _source; } }

        /// <summary>
        /// The date and time the event was emitted
        /// </summary>
        public DateTime Timestamp { get { return _timestamp; } }

        /// <summary>
        /// The type of diagnostic event
        /// </summary>
        public DiagnosticEventType Type { get { return _type; } }

        /// <summary>
        /// Specific details regarding this instance of the event
        /// </summary>
        public string Detail { get { return _detail; } }

        /// <summary>
        /// The exception related to the event, if applicable
        /// </summary>
        public Exception Exception { get { return _exception; } }

        /// <summary>
        /// The message to which the event pertains, if applicable
        /// </summary>
        public Message Message { get { return _message; } }

        /// <summary>
        /// The name of the endpoint, if applicable
        /// </summary>
        public EndpointName Endpoint { get { return _endpoint; } }

        /// <summary>
        /// The queue to which the message pertains, if applicable
        /// </summary>
        public QueueName Queue { get { return _queue; } }

        /// <summary>
        /// The topic to which the message pertains, if applicable
        /// </summary>
        public TopicName Topic { get { return _topic; } }

        /// <summary>
        /// Initializes a new <see cref="DiagnosticEvent"/>
        /// </summary>
        /// <param name="source">The object that emitted the event</param>
        /// <param name="type">The type of event</param>
        /// <param name="detail">Specific details regarding this instance of the event</param>
        /// <param name="exception">The exception related to the event, if applicable</param>
        /// <param name="message">The message to which the event pertains, if applicable</param>
        /// <param name="endpoint">The name of the endpoint, if applicable</param>
        /// <param name="queue">The queue to which the event pertains, if applicable</param>
        /// <param name="topic">The topic to which the message pertains, if applicable</param>
        public DiagnosticEvent(object source, DiagnosticEventType type, string detail = null, Exception exception = null, Message message = null, EndpointName endpoint = null, QueueName queue = null, TopicName topic = null)
        {
            if (type == null) throw new ArgumentNullException("type");
            _timestamp = DateTime.UtcNow;
            _source = source;
            _type = type;
            _detail = detail;
            _exception = exception;
            _message = message;
            _endpoint = endpoint;
            _queue = queue;
            _topic = topic;
        }
    }
}
