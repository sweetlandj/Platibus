using System;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// A <see cref="IDiagnosticEventSink"/> implementation that invokes a callback when a
    /// <see cref="DiagnosticEvent"/> is emitted
    /// </summary>
    public class DelegateDiagnosticEventSink : IDiagnosticEventSink
    {
        private readonly Action<DiagnosticEvent> _consume;

        /// <summary>
        /// Initializes a new <see cref="DelegateDiagnosticEventSink"/> with the specified
        /// callback.
        /// </summary>
        /// <param name="consume">The callback to invoke when a diagnostic event is 
        /// emitted</param>
        public DelegateDiagnosticEventSink(Action<DiagnosticEvent> consume)
        {
            _consume = consume ?? throw new ArgumentNullException(nameof(consume));
        }

        /// <inheritdoc />
        public void Consume(DiagnosticEvent @event)
        {
            _consume(@event);
        }

        /// <inheritdoc />
        public Task ConsumeAsync(DiagnosticEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            _consume(@event);
#if NETSTANDARD2_0
            return Task.CompletedTask;
#endif
#if NET452
            return Task.FromResult(false);
#endif
        }
    }
}
