using System;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// Arguments for events raised in response to unhandled exceptions thrown from 
    /// <see cref="IDiagnosticEventSink"/> implementations while handling events
    /// </summary>
    public class DiagnosticSinkExceptionEventArgs : EventArgs
    {
        private readonly DiagnosticEvent _diagnosticEvent;
        private readonly IDiagnosticEventSink _diagnosticEventSink;
        private readonly Exception _exception;

        /// <summary>
        /// The event being handled
        /// </summary>
        public DiagnosticEvent DiagnosticEvent { get { return _diagnosticEvent; } }

        /// <summary>
        /// The data sink from which the exception was thrown
        /// </summary>
        public IDiagnosticEventSink DiagnosticEventSink { get { return _diagnosticEventSink; } }
         
        /// <summary>
        /// The unhandled exception
        /// </summary>
        public Exception Exception { get { return _exception; } }

        /// <summary>
        /// Initializes a new <see cref="DiagnosticSinkExceptionEventArgs"/>
        /// </summary>
        /// <param name="diagnosticEvent">The event being handled</param>
        /// <param name="diagnosticEventSink">The data sink from which the exception was thrown</param>
        /// <param name="exception">The unhandled exception</param>
        public DiagnosticSinkExceptionEventArgs(DiagnosticEvent diagnosticEvent, IDiagnosticEventSink diagnosticEventSink, Exception exception)
        {
            _diagnosticEvent = diagnosticEvent;
            _diagnosticEventSink = diagnosticEventSink;
            _exception = exception;
        }
    }
}
