using Platibus.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Platibus.UnitTests
{
    public class VerificationSink : IDiagnosticEventSink
    {
        private readonly object _syncRoot = new object();
        private readonly IList<DiagnosticEvent> _consumedEvents = new List<DiagnosticEvent>();

        public IEnumerable<DiagnosticEvent> ConsumedEvents
        {
            get
            {
                lock (_syncRoot)
                {
                    return new List<DiagnosticEvent>(_consumedEvents);
                }
            }
        }

        public void Consume(DiagnosticEvent @event)
        {
            lock (_syncRoot)
            {
                _consumedEvents.Add(@event);
            }
        }

        public Task ConsumeAsync(DiagnosticEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (_syncRoot)
            {
                _consumedEvents.Add(@event);
            }

            return Task.FromResult(0);
        }

        public void AssertNone(DiagnosticEventType type)
        {
            AssertNone<DiagnosticEvent>(type);
        }

        public void AssertExactly(int count, DiagnosticEventType type)
        {
            AssertExactly<DiagnosticEvent>(count, e => e.Type == type);
        }

        public void AssertAtLeast(int count, DiagnosticEventType type)
        {
            AssertAtLeast<DiagnosticEvent>(count, e => e.Type == type);
        }

        public void AssertNoMoreThan(int count, DiagnosticEventType type)
        {
            AssertNoMoreThan<DiagnosticEvent>(count, e => e.Type == type);
        }

        public void AssertNone(DiagnosticEventType type, Message message)
        {
            AssertExactly<DiagnosticEvent>(0, e => e.Type == type && e.Message?.Headers.MessageId == message.Headers.MessageId);
        }

        public void AssertExactly(int count, DiagnosticEventType type, Message message)
        {
            AssertExactly<DiagnosticEvent>(count, e => e.Type == type && e.Message?.Headers.MessageId == message.Headers.MessageId);
        }

        public void AssertAtLeast(int count, DiagnosticEventType type, Message message)
        {
            AssertAtLeast<DiagnosticEvent>(count, e => e.Type == type && e.Message?.Headers.MessageId == message.Headers.MessageId);
        }

        public void AssertNoMoreThan(int count, DiagnosticEventType type, Message message)
        {
            AssertNoMoreThan<DiagnosticEvent>(count, e => e.Type == type && e.Message?.Headers.MessageId == message.Headers.MessageId);
        }

        public void AssertNone<TEvent>(DiagnosticEventType type)
            where TEvent : DiagnosticEvent
        {
            AssertExactly<TEvent>(0, e => e.Type == type);
        }

        public void AssertExactly<TEvent>(int count, DiagnosticEventType type)
            where TEvent : DiagnosticEvent
        {
            AssertExactly<TEvent>(count, e => e.Type == type);
        }

        public void AssertAtLeast<TEvent>(int count, DiagnosticEventType type)
            where TEvent : DiagnosticEvent
        {
            AssertBetween<TEvent>(count, int.MaxValue, e => e.Type == type);
        }

        public void AssertNoMoreThan<TEvent>(int count, DiagnosticEventType type)
            where TEvent : DiagnosticEvent
        {
            AssertBetween<TEvent>(0, count, e => e.Type == type);
        }

        public void AssertNone<TEvent>(DiagnosticEventType type, Message message)
            where TEvent : DiagnosticEvent
        {
            AssertExactly<TEvent>(0, e => e.Type == type && e.Message?.Headers.MessageId == message.Headers.MessageId);
        }

        public void AssertExactly<TEvent>(int count, DiagnosticEventType type, Message message)
            where TEvent : DiagnosticEvent
        {
            AssertExactly<TEvent>(count, e => e.Type == type && e.Message?.Headers.MessageId == message.Headers.MessageId);
        }

        public void AssertAtLeast<TEvent>(int count, DiagnosticEventType type, Message message)
            where TEvent : DiagnosticEvent
        {
            AssertBetween<TEvent>(count, int.MaxValue, e => e.Type == type && e.Message?.Headers.MessageId == message.Headers.MessageId);
        }

        public void AssertNoMoreThan<TEvent>(int count, DiagnosticEventType type, Message message)
            where TEvent : DiagnosticEvent
        {
            AssertBetween<TEvent>(0, count, e => e.Type == type && e.Message?.Headers.MessageId == message.Headers.MessageId);
        }

        public void AssertExactly(int count, Func<DiagnosticEvent, bool> predicate)
        {
            AssertBetween<DiagnosticEvent>(count, count, predicate);
        }

        public void AssertAtLeast(int count, Func<DiagnosticEvent, bool> predicate)
        {
            AssertBetween<DiagnosticEvent>(count, int.MaxValue, predicate);
        }

        public void AssertNoMoreThan(int count, Func<DiagnosticEvent, bool> predicate)
        {
            AssertBetween<DiagnosticEvent>(0, count, predicate);
        }

        public void AssertBetween(int min, int max, Func<DiagnosticEvent, bool> predicate)
        {
            AssertBetween<DiagnosticEvent>(min, max, predicate);
        }

        public void AssertExactly<TEvent>(int count, Func<TEvent, bool> predicate)
        {
            AssertBetween(count, count, predicate);
        }

        public void AssertAtLeast<TEvent>(int count, Func<TEvent, bool> predicate)
        {
            AssertBetween(count, int.MaxValue, predicate);
        }

        public void AssertNoMoreThan<TEvent>(int count, Func<TEvent, bool> predicate)
        {
            AssertBetween(0, count, predicate);
        }

        public void AssertBetween<TEvent>(int min, int max, Func<TEvent, bool> predicate)
        {
            var count = ConsumedEvents.OfType<TEvent>().Count(predicate);
            Assert.True(count >= min, $"Expected count >= {min} but was {count}");
            Assert.True(count <= max, $"Expected count <= {max} but was {count}");
        }
    }
}
