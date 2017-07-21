using System.Collections.Generic;
using Platibus.Diagnostics;
using Platibus.Http;

namespace Platibus.UnitTests.Diagnostics
{
    public class DiagnosticFakes
    {
        private readonly object _source;

        public DiagnosticFakes(object source)
        {
            _source = source ?? this;
        }
        
        public IEnumerable<DiagnosticEvent> NominalMessageFlow()
        {
            return new[]
            {
                new HttpEvent(_source, HttpEventType.HttpRequestReceived),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageReceived),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageAcknowledged),
                new HttpEvent(_source, HttpEventType.HttpResponseSent)
            };
        }

        public IEnumerable<DiagnosticEvent> NominalMessageFlowWithAcknowledgementFailure()
        {
            return new[]
            {
                new HttpEvent(_source, HttpEventType.HttpRequestReceived),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageReceived),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new HttpEvent(_source, HttpEventType.HttpResponseSent)
            };
        }

        public IEnumerable<DiagnosticEvent> QueuedMessageFlow()
        {
            return new[]
            {
                new HttpEvent(_source, HttpEventType.HttpRequestReceived),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageReceived),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageEnqueued),
                new HttpEvent(_source, HttpEventType.HttpResponseSent),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageAcknowledged)
            };
        }

        public IEnumerable<DiagnosticEvent> NominalMessageFlowWithReply()
        {
            return new[]
            {
                new HttpEvent(_source, HttpEventType.HttpRequestReceived),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageReceived),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageSent),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageDelivered),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageAcknowledged),
                new HttpEvent(_source, HttpEventType.HttpResponseSent)
            };
        }

        public IEnumerable<DiagnosticEvent> QueuedMessageFlowWithReply()
        {
            return new[]
            {
                new HttpEvent(_source, HttpEventType.HttpRequestReceived),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageReceived),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageEnqueued),
                new HttpEvent(_source, HttpEventType.HttpResponseSent),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageSent),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageDelivered),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageAcknowledged)
            };
        }

        public IEnumerable<DiagnosticEvent> QueuedMessageFlowWithAcknowledgementFailure()
        {
            return new[]
            {
                new HttpEvent(_source, HttpEventType.HttpRequestReceived),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageReceived),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageEnqueued),
                new HttpEvent(_source, HttpEventType.HttpResponseSent),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.MaxAttemptsExceeded)
            };
        }

        public IEnumerable<DiagnosticEvent> QueuedMessageFlowWithUnhandledException()
        {
            return new[]
            {
                new HttpEvent(_source, HttpEventType.HttpRequestReceived),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageReceived),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageEnqueued),
                new HttpEvent(_source, HttpEventType.HttpResponseSent),
                new DiagnosticEvent(_source, DiagnosticEventType.UnhandledException),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.UnhandledException),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.UnhandledException),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.UnhandledException),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.UnhandledException),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.UnhandledException),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.UnhandledException),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.UnhandledException),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.UnhandledException),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.UnhandledException),
                new DiagnosticEvent(_source, DiagnosticEventType.MessageNotAcknowledged),
                new DiagnosticEvent(_source, DiagnosticEventType.MaxAttemptsExceeded)
            };
        }
    }
}
