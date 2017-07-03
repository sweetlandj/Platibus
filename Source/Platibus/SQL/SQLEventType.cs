using Platibus.Diagnostics;

namespace Platibus.SQL
{
    /// <summary>
    /// Diagnostic event types related to SQL events
    /// </summary>
    public static class SQLEventType
    {
        /// <summary>
        /// Emitted when a connection is opened
        /// </summary>
        public static readonly DiagnosticEventType ConnectionOpened = new DiagnosticEventType("ConnectionOpened", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted when a connection is closed
        /// </summary>
        public static readonly DiagnosticEventType ConnectionClosed = new DiagnosticEventType("ConnectionClosed", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted when there is an error opening or closing a connection
        /// </summary>
        public static readonly DiagnosticEventType ConnectionError = new DiagnosticEventType("ConnectionError", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted when there is an error opening or closing a connection
        /// </summary>
        public static readonly DiagnosticEventType CommandExecuted = new DiagnosticEventType("CommandExecuted", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted when there is an error opening or closing a connection
        /// </summary>
        public static readonly DiagnosticEventType CommandError = new DiagnosticEventType("CommandError", DiagnosticEventLevel.Error);
    }
}
