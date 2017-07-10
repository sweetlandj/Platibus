namespace Platibus.Diagnostics
{
    /// <summary>
    /// Delegate that handles events raised in response to unhandled exceptions thrown from 
    /// <see cref="IDiagnosticEventSink"/> implementations while handling events
    /// </summary>
    /// <param name="source">The object that raised the event</param>
    /// <param name="args">The event arguments</param>
    public delegate void DiagnosticSinkExceptionHandler(object source, DiagnosticSinkExceptionEventArgs args);
    
}
