using System;
using Platibus.Diagnostics;

namespace Platibus.Http
{
    /// <summary>
    /// Mutable factory object that produces immutable diagnostic events for HTTP operations
    /// </summary>
    public class HttpEventBuilder : DiagnosticEventBuilder
    {
        /// <summary>
        /// The HTTP method specified in the request
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// The IP or hostname of the remote client
        /// </summary>
        public string Remote { get; set; }

        /// <summary>
        /// The request URI
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// The HTTP status code in the response
        /// </summary>
        public int? Status { get; set; }
        
        /// <summary>
        /// Initializes a new <see cref="HttpEventBuilder"/>
        /// </summary>
        /// <param name="source">The object that will produce the event</param>
        /// <param name="type">The type of event</param>
        public HttpEventBuilder(object source, DiagnosticEventType type) : base(source, type)
        {
        }

        /// <inheritdoc />
        public override DiagnosticEvent Build()
        {
            return new HttpEvent(Source, Type, Detail, Exception, Message, Endpoint, Queue, Topic, Remote, Method, Uri, Status);
        }
    }
}
