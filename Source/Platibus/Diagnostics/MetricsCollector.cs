// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Http;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// A <see cref="IDiagnosticService"/> implementation that tracks key metrics to report on
    /// the activity and health of the instance
    /// </summary>
    public class MetricsCollector : IDiagnosticEventSink, IDisposable
    {
        private readonly TimeSpan _sampleRate;
        private readonly Timer _sampleTimer;
        private readonly Metrics _total = new Metrics();

        private Metrics _current = new Metrics();
        private Metrics _sample = new Metrics();
        
        private bool _disposed;

        /// <summary>
        /// Returns the latest sample metrics normalized per second
        /// </summary>
        public Dictionary<string, double> Sample
        {
            get
            {
                var total = _total;
                var sample = _sample;
                var sampleRateInSeconds = _sampleRate.TotalSeconds;
                return new Dictionary<string, double>
                {
                    {"TotalRequests", total.Requests},
                    {"TotalReceived", total.Received},
                    {"TotalAcknowledgements", total.Acknowledgements},
                    {"TotalAcknowledgementFailures", total.AcknowledgementFailures},
                    {"TotalExpired", total.Expired},
                    {"TotalDead", total.Dead},
                    {"TotalSent", total.Sent},
                    {"TotalPublished", total.Published},
                    {"TotalDelivered", total.Delivered},
                    {"TotalDeliveryFailures", total.DeliveryFailures},
                    {"TotalErrors", total.Errors},
                    {"TotalWarnings", total.Warnings},

                    {"RequestsPerSecond", sample.Requests/sampleRateInSeconds},
                    {"ReceivedPerSecond", sample.Received/sampleRateInSeconds},
                    {"AcknowledgementsPerSecond", sample.Acknowledgements/sampleRateInSeconds},
                    {"AcknowledgementFailuresPerSecond", sample.AcknowledgementFailures/sampleRateInSeconds},
                    {"ExpiredPerSecond", sample.Expired/sampleRateInSeconds},
                    {"DeadPerSecond", sample.Dead/sampleRateInSeconds},
                    {"SentPerSecond", sample.Sent/sampleRateInSeconds},
                    {"PublishedPerSecond", sample.Published/sampleRateInSeconds},
                    {"DeliveredPerSecond", sample.Delivered/sampleRateInSeconds},
                    {"DeliveryFailuresPerSecond", sample.DeliveryFailures/sampleRateInSeconds},
                    {"ErrorsPerSecond", sample.Errors/sampleRateInSeconds},
                    {"WarningsPerSecond", sample.Warnings/sampleRateInSeconds}
                };
            }
        }

        /// <summary>
        /// Initializes a new <see cref="MetricsCollector"/>
        /// </summary>
        /// <param name="sampleRate">(Optional) The rate at which samples should be taken</param>
        /// <remarks>
        /// The default sample rate is 5 seconds
        /// </remarks>
        public MetricsCollector(TimeSpan sampleRate = default(TimeSpan))
        {
            if (sampleRate <= TimeSpan.Zero)
            {
                sampleRate = TimeSpan.FromSeconds(5);
            }
            _sampleRate = sampleRate;
            _sampleTimer = new Timer(_ => TakeSample(), null, TimeSpan.FromSeconds(1), _sampleRate);
        }
        
        /// <inheritdoc />
        public void Consume(DiagnosticEvent @event)
        {
            IncrementCounters(@event);
        }

        /// <inheritdoc />
        public Task ConsumeAsync(DiagnosticEvent @event, CancellationToken cancellationToken = new CancellationToken())
        {
            IncrementCounters(@event);
            return Task.FromResult(0);
        }

        private void IncrementCounters(DiagnosticEvent @event)
        {
            switch (@event.Type.Level)
            {
                case DiagnosticEventLevel.Error:
                    Interlocked.Increment(ref _current.Errors);
                    Interlocked.Increment(ref _total.Errors);
                    break;
                case DiagnosticEventLevel.Warn:
                    Interlocked.Increment(ref _current.Warnings);
                    Interlocked.Increment(ref _total.Warnings);
                    break;
            }

            if (@event.Type == HttpEventType.HttpRequestReceived)
            {
                Interlocked.Increment(ref _current.Requests);
                Interlocked.Increment(ref _total.Requests);
            }
            else if (@event.Type == DiagnosticEventType.MessageReceived)
            {
                Interlocked.Increment(ref _current.Received);
                Interlocked.Increment(ref _total.Received);
            }
            else if (@event.Type == DiagnosticEventType.MessageAcknowledged)
            {
                Interlocked.Increment(ref _current.Acknowledgements);
                Interlocked.Increment(ref _total.Acknowledgements);
            }
            else if (@event.Type == DiagnosticEventType.MessageNotAcknowledged)
            {
                Interlocked.Increment(ref _current.AcknowledgementFailures);
                Interlocked.Increment(ref _total.AcknowledgementFailures);
            }
            else if (@event.Type == DiagnosticEventType.MessageSent)
            {
                Interlocked.Increment(ref _current.Sent);
                Interlocked.Increment(ref _total.Sent);
            }
            else if (@event.Type == DiagnosticEventType.MessagePublished)
            {
                Interlocked.Increment(ref _current.Published);
                Interlocked.Increment(ref _total.Published);
            }
            else if (@event.Type == DiagnosticEventType.MessageDelivered)
            {
                Interlocked.Increment(ref _current.Delivered);
                Interlocked.Increment(ref _total.Delivered);
            }
            else if (@event.Type == DiagnosticEventType.MessageDeliveryFailed)
            {
                Interlocked.Increment(ref _current.DeliveryFailures);
                Interlocked.Increment(ref _total.DeliveryFailures);
            }
            else if (@event.Type == DiagnosticEventType.MessageExpired)
            {
                Interlocked.Increment(ref _current.Expired);
                Interlocked.Increment(ref _total.Expired);
            }
            else if (@event.Type == DiagnosticEventType.DeadLetter)
            {
                Interlocked.Increment(ref _current.Dead);
                Interlocked.Increment(ref _total.Dead);
            }
        }

        private void TakeSample()
        {
            var current = Interlocked.Exchange(ref _current, new Metrics());
            Interlocked.Exchange(ref _sample, current);
        }

        /// <summary>
        /// Releases or frees managed and unmanaged resources
        /// </summary>
        /// <param name="disposing">Whether this method is called from <see cref="Dispose()"/></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sampleTimer.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        ~MetricsCollector()
        {
            Dispose(false);
        }
        
        private class Metrics
        {
            public long Requests;
            public long Received;
            public long Acknowledgements;
            public long AcknowledgementFailures;
            public long Expired;
            public long Dead;
            public long Sent;
            public long Published;
            public long Delivered;
            public long DeliveryFailures;
            public long Errors;
            public long Warnings;
        }
    }
}
