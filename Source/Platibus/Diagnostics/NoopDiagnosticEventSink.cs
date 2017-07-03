using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// Default diagnostic event handler that performs no action when an event is received
    /// </summary>
    public class NoopDiagnosticEventSink : IDiagnosticEventSink
    {
        /// <summary>
        /// The singleton instance of the <see cref="NoopDiagnosticEventSink"/>
        /// </summary>
        public static readonly IDiagnosticEventSink Instance = new NoopDiagnosticEventSink();

        private NoopDiagnosticEventSink()
        {
        }

        /// <inheritdoc />
        public void Receive(DiagnosticEvent @event)
        {
        }

        /// <inheritdoc />
        public Task ReceiveAsync(DiagnosticEvent @event, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(0);
        }
    }
}
