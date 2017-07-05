using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// Interface through which Platibus reports diagnostic events
    /// </summary>
    /// <remarks>
    /// Implementations of this interface must be threadsafe.
    /// </remarks>
    public interface IDiagnosticEventSink
    {
        /// <summary>
        ///  Receives and handles a diagnostic event
        /// </summary>
        /// <param name="event">The diagnostic event to consume</param>
        void Receive(DiagnosticEvent @event);

        /// <summary>
        /// Receives and handles a diagnostic event asynchronously
        /// </summary>
        /// <param name="event">The diagnostic event to consume</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be provided
        /// by the caller to interrupt processing of the diagnostic event</param>
        Task ReceiveAsync(DiagnosticEvent @event, CancellationToken cancellationToken = default(CancellationToken));
    }
}
