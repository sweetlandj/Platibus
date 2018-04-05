using Platibus.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.SampleWebApp.Controllers
{
    public class DiagnosticEventLog : IDiagnosticEventSink
    {
        public static readonly DiagnosticEventLog SingletonInstance = new DiagnosticEventLog();

        private readonly object _syncRoot = new object();

        private readonly LinkedList<DiagnosticEvent> _emittedEvents = new LinkedList<DiagnosticEvent>();

        public IEnumerable<DiagnosticEvent> EmittedEvents
        {
            get
            {
                lock (_emittedEvents)
                {
                    return new List<DiagnosticEvent>(_emittedEvents);
                }
            }
        }

        public void Consume(DiagnosticEvent @event)
        {
            lock(_syncRoot)
            {
                _emittedEvents.AddFirst(@event);
                while (_emittedEvents.Count > 1000)
                {
                    _emittedEvents.RemoveLast();
                }
            }
        }

        public Task ConsumeAsync(DiagnosticEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            Consume(@event);
            return Task.FromResult(0);
        }

        private DiagnosticEventLog()
        {
        }
    }
}