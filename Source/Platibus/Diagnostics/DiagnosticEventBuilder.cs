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
    /// Mutable factory object that produces immutable diagnostic events
    /// </summary>
    public class DiagnosticEventBuilder
    {
        /// <summary>
        /// The object that will produce the event
        /// </summary>
        protected readonly object Source;

        /// <summary>
        /// The type of event
        /// </summary>
        protected readonly DiagnosticEventType Type;
        
        /// <summary>
        /// Specific details regarding this instance of the event
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// The exception related to the event, if applicable
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// The message to which the event pertains, if applicable
        /// </summary>
        public Message Message { get; set; }

        /// <summary>
        /// The name of the endpoint, if applicable
        /// </summary>
        public EndpointName Endpoint { get; set; }

        /// <summary>
        /// The queue to which the message pertains, if applicable
        /// </summary>
        public QueueName Queue { get; set; }

        /// <summary>
        /// The topic to which the message pertains, if applicable
        /// </summary>
        public TopicName Topic { get; set; }

        /// <summary>
        /// Initializes a new <see cref="DiagnosticEventBuilder"/> for the specified 
        /// </summary>
        /// <param name="source">The object that will produce the event</param>
        /// <param name="type">The type of event</param>
        public DiagnosticEventBuilder(object source, DiagnosticEventType type)
        {
            Source = source;
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>
        /// Builds the diagnostic event
        /// </summary>
        /// <returns>Returns the newly constructed diagnostic event</returns>
        public virtual DiagnosticEvent Build()
        {
            return new DiagnosticEvent(Source, Type, Detail, Exception, Message, Endpoint, Queue, Topic);
        }
    }
}
