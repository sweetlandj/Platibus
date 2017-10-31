using System;
using Platibus.Diagnostics;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Platibus.UnitTests
{
    internal class VerificationSink : IDiagnosticEventSink
    {
        private readonly ConcurrentQueue<DiagnosticEvent> _emittedEvents = new ConcurrentQueue<DiagnosticEvent>();

        public void Consume(DiagnosticEvent @event)
        {
            _emittedEvents.Enqueue(@event);
        }

        public Task ConsumeAsync(DiagnosticEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            _emittedEvents.Enqueue(@event);
            return Task.FromResult(0);
        }

        public void VerifyEmitted(DiagnosticEventType type)
        {
            Assert.NotEmpty(_emittedEvents.Where(e => e.Type == type));
        }
        
        public void VerifyEmitted(DiagnosticEventType type, Func<DiagnosticEvent, bool> predicate)
        {
            VerifyEmitted<DiagnosticEvent>(type, predicate);
        }

        public void VerifyEmitted<TEvent>(DiagnosticEventType type, Func<TEvent, bool> predicate) where TEvent : DiagnosticEvent
        {
            if (predicate == null)
            {
                VerifyEmitted(type);
                return;
            };
            Assert.NotEmpty(_emittedEvents.OfType<TEvent>().Where(e => e.Type == type && predicate(e)));
        }
    }
}
