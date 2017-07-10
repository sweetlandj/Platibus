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
    public interface IDiagnosticService
    {
        /// <summary>
        /// Registers a data sink to receive a copy of each diagnostic event that is emitted
        /// </summary>
        /// <param name="sink"></param>
        void AddConsumer(IDiagnosticEventSink sink);

        /// <summary>
        ///  Receives and handles a diagnostic event
        /// </summary>
        /// <param name="event">The diagnostic event to consume</param>
        void Emit(DiagnosticEvent @event);

        /// <summary>
        /// Receives and handles a diagnostic event asynchronously
        /// </summary>
        /// <param name="event">The diagnostic event to consume</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be provided
        /// by the caller to interrupt processing of the diagnostic event</param>
        Task EmitAsync(DiagnosticEvent @event, CancellationToken cancellationToken = default(CancellationToken));
    }
}
