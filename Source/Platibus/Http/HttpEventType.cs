using Platibus.Diagnostics;

namespace Platibus.Http
{
    /// <summary>
    /// <see cref="DiagnosticEventType"/>s specific to HTTP events
    /// </summary>
    public static class HttpEventType
    {
        /// <summary>
        /// Emitted whenever an HTTP listener starts
        /// </summary>
        public static DiagnosticEventType HttpServerStarted = new DiagnosticEventType("HttpServerStarted", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted whenever an HTTP listener is stopped
        /// </summary>
        public static DiagnosticEventType HttpServerStopped = new DiagnosticEventType("HttpServerStopped", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted whenever there is an error related to the HTTP server
        /// </summary>
        public static DiagnosticEventType HttpServerError = new DiagnosticEventType("HttpServerError", DiagnosticEventLevel.Error);

        /// <summary>
        /// Emitted whenever an HTTP request is received
        /// </summary>
        public static DiagnosticEventType HttpRequestReceived = new DiagnosticEventType("HttpRequestReceived", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted whenever a message is sent via HTTP
        /// </summary>
        public static DiagnosticEventType HttpMessageSent = new DiagnosticEventType("HttpMessageSent", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted whenever an HTTP subscription request is sent
        /// </summary>
        public static DiagnosticEventType HttpSubscriptionRequestSent = new DiagnosticEventType("HttpSubscriptionRequestSent", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted whenever an HTTP response is sent
        /// </summary>
        public static DiagnosticEventType HttpResponseSent = new DiagnosticEventType("HttpResponseSent", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted in response to low-level HTTP communcation errors
        /// </summary>
        public static DiagnosticEventType HttpCommunicationError = new DiagnosticEventType("HttpCommunicationError", DiagnosticEventLevel.Error);

        /// <summary>
        /// Emitted whenever a message addressed to the local server is handled locally instead of
        /// going through the full HTTP request/response process.
        /// </summary>
        public static DiagnosticEventType HttpTransportBypassed = new DiagnosticEventType("HttpTransportBypassed", DiagnosticEventLevel.Info);

        
    }
}
