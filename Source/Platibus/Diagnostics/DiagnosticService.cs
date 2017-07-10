using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// A <see cref="IDiagnosticService"/> that passes a copy of every
    /// <see cref="DiagnosticEvent"/> it receives to a list of other registered
    /// <see cref="IDiagnosticEventSink"/>s
    /// </summary>
    public class DiagnosticService : IDiagnosticService, IEnumerable<IDiagnosticEventSink>
    {
        /// <summary>
        /// The default diagnostics service to which events will be reported
        /// </summary>
        /// <remarks>
        /// This is a singleton instance that provides a convenient mechanism through which
        /// consumers can register event sinks early without custom configuration handling.
        /// </remarks>
        public static readonly IDiagnosticService DefaultInstance = new DiagnosticService();

        private readonly ConcurrentBag<IDiagnosticEventSink> _consumers = new ConcurrentBag<IDiagnosticEventSink>();

        /// <summary>
        /// Registers a data sink to receive a copy of each diagnostic event that is emitted
        /// </summary>
        /// <param name="sink"></param>
        public void AddConsumer(IDiagnosticEventSink sink)
        {
            if (sink != null) _consumers.Add(sink);
        }

        /// <inheritdoc />
        public void Emit(DiagnosticEvent @event)
        {
            foreach (var sink in _consumers)
            {
                try
                {
                    sink.Consume(@event);
                }
                catch (Exception)
                {
                }
            }
        }

        /// <inheritdoc />
        public Task EmitAsync(DiagnosticEvent @event, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var receiveTasks = _consumers.Select(sink => sink.ConsumeAsync(@event, cancellationToken));
                return Task.WhenAll(receiveTasks);
            }
            catch (Exception)
            {
            }
            return Task.FromResult(0);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _consumers).GetEnumerator();
        }

        /// <inheritdoc />
        public IEnumerator<IDiagnosticEventSink> GetEnumerator()
        {
            return _consumers.GetEnumerator();
        }
    }
}
