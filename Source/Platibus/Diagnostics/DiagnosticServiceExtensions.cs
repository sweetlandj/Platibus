using System;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// Extension methods for working with <see cref="IDiagnosticService"/> and 
    /// <see cref="IDiagnosticEventSink"/> implementations.
    /// </summary>
    public static class DiagnosticServiceExtensions
    {
        /// <summary>
        /// Adds a new <see cref="IDiagnosticEventSink"/> that invokes the specified callback
        /// when diagnostic events are emitted.
        /// </summary>
        /// <param name="diagnosticService">The diagnostic service to which the sink is to be
        /// added</param>
        /// <param name="consume">The callback used to consume the diagnostic events emitted
        /// throug the <paramref name="diagnosticService"/></param>
        public static void AddSink(this IDiagnosticService diagnosticService, Action<DiagnosticEvent> consume)
        {
            diagnosticService.AddSink(new DelegateDiagnosticEventSink(consume));
        }
    }
}
