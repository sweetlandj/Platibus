using System.Threading.Tasks;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// Interface through which Platibus reports diagnostic events
    /// </summary>
    public interface IDiagnosticEventSink
    {
        /// <summary>
        /// Raises a diagnostic event
        /// </summary>
        /// <param name="event">The event</param>
        Task Receive(DiagnosticEvent @event);
    }
}
