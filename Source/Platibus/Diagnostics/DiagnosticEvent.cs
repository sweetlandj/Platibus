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
        /// <summary>
        /// The object that emitted the event
        /// </summary>
        public object Source { get; }

        /// <summary>
        /// The date and time the event was emitted
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// The type of diagnostic event
        /// </summary>
        public DiagnosticEventType Type { get; }

        /// <summary>
        /// Specific details regarding this instance of the event
        /// </summary>
        public string Detail { get; }

        /// <summary>
        /// The exception related to the event, if applicable
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// The message to which the event pertains, if applicable
        /// </summary>
        public Message Message { get; }

        /// <summary>
        /// The name of the endpoint, if applicable
        /// </summary>
        public EndpointName Endpoint { get; }

        /// <summary>
        /// The queue to which the message pertains, if applicable
        /// </summary>
        public QueueName Queue { get; }

        /// <summary>
        /// The topic to which the message pertains, if applicable
        /// </summary>
        public TopicName Topic { get; }

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
            Timestamp = DateTime.UtcNow;
            Source = source;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Detail = detail;
            Exception = exception;
            Message = message;
            Endpoint = endpoint;
            Queue = queue;
            Topic = topic;
        }
    }
}
