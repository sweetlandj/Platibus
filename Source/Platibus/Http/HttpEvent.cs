using System;
using Platibus.Diagnostics;

namespace Platibus.Http
{
    /// <summary>
    /// A type of <see cref="DiagnosticEvent"/> specific to HTTP processing
    /// </summary>
    public class HttpEvent : DiagnosticEvent
    {
        /// <summary>
        /// The IP or hostname of the remote client
        /// </summary>
        public string Remote { get; }

        /// <summary>
        /// The HTTP method specified in the request
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// The request URI
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// The HTTP status code in the response
        /// </summary>
        public int? Status { get; }

        /// <summary>
        /// Initializes a new <see cref="HttpEvent"/>
        /// </summary>
        /// <param name="source">The object that emitted the event</param>
        /// <param name="type">The type of event</param>
        /// <param name="detail">Specific details regarding this instance of the event</param>
        /// <param name="exception">The exception related to the event, if applicable</param>
        /// <param name="message">The message to which the event pertains, if applicable</param>
        /// <param name="endpoint">The name of the endpoint, if applicable</param>
        /// <param name="queue">The queue to which the event pertains, if applicable</param>
        /// <param name="topic">The topic to which the message pertains, if applicable</param>
        /// <param name="remote">The IP or hostname of the remote client</param>
        /// <param name="method">The HTTP method specified in the request</param>
        /// <param name="uri">The request URI</param>
        /// <param name="status">The HTTP status code in the response</param>
        public HttpEvent(object source, DiagnosticEventType type, string detail = null, Exception exception = null, Message message = null, EndpointName endpoint = null, QueueName queue = null, TopicName topic = null, string remote = null, string method = null, Uri uri = null, int? status = null) 
            : base(source, type, detail, exception, message, endpoint, queue, topic)
        {
            Remote = remote;
            Method = method;
            Uri = uri;
            Status = status;
        }
    }
}
