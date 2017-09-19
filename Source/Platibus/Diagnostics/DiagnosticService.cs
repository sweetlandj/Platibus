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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Diagnostics
{
    /// <inheritdoc cref="IDiagnosticService" />
    /// <summary>
    /// A <see cref="T:Platibus.Diagnostics.IDiagnosticService" /> that passes a copy of every
    /// <see cref="T:Platibus.Diagnostics.DiagnosticEvent" /> it receives to a list of other registered
    /// <see cref="T:Platibus.Diagnostics.IDiagnosticEventSink" />s
    /// </summary>
    public class DiagnosticService : IDiagnosticService, IEnumerable<IDiagnosticEventSink>
    {
        private readonly object _syncRoot = new object();

        /// <summary>
        /// The default diagnostics service to which events will be reported
        /// </summary>
        /// <remarks>
        /// This is a singleton instance that provides a convenient mechanism through which
        /// consumers can register event sinks early without custom configuration handling.
        /// </remarks>
        public static readonly IDiagnosticService DefaultInstance = new DiagnosticService();

        private volatile IList<IDiagnosticEventSink> _consumers = new List<IDiagnosticEventSink>();

        /// <summary>
        /// Event raised when an unhandled exception is caught from one of the registered sinks
        /// </summary>
        public event DiagnosticSinkExceptionHandler Exception;

        /// <inheritdoc />
        public void AddSink(IDiagnosticEventSink sink)
        {
            if (sink == null) return;
            lock(_syncRoot)
            {
                _consumers = new List<IDiagnosticEventSink>(_consumers) {sink};
            }
        }

        /// <inheritdoc />
        public void RemoveSink(IDiagnosticEventSink sink)
        {
            if (sink == null) return;
            lock (_syncRoot)
            {
                _consumers = _consumers.Where(c => c != sink).ToList();
            }
        }

        /// <inheritdoc />
        public void Emit(DiagnosticEvent @event)
        {
            var myConsumers = _consumers;
            foreach (var sink in myConsumers)
            {
                try
                {
                    sink.Consume(@event);
                }
                catch (Exception ex)
                {
                    RaiseExceptionEvent(@event, sink, ex);
                }
            }
        }

        /// <inheritdoc />
        public Task EmitAsync(DiagnosticEvent @event, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var receiveTasks = _consumers.Select(async sink =>
                {
                    try
                    {
                        await sink.ConsumeAsync(@event, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        RaiseExceptionEvent(@event, sink, ex);
                    }
                });
                return Task.WhenAll(receiveTasks);
            }
            catch (Exception)
            {
                // Ignored
            }
            return Task.FromResult(0);
        }
        
        /// <summary>
        /// Safely raises the <see cref="Exception"/> event to invoke registered 
        /// <see cref="DiagnosticSinkExceptionHandler"/> delegates
        /// </summary>
        /// <remarks>
        /// Exceptions thrown from <see cref="DiagnosticSinkExceptionHandler"/> delegates are 
        /// swallowed and ignored.
        /// </remarks>
        /// <param name="event">The diagnostic event that was being consumed at the time the
        /// exception occurred</param>
        /// <param name="sink">The sink from which the unhandled exception was caught</param>
        /// <param name="ex">The unhandled exception</param>
        protected void RaiseExceptionEvent(DiagnosticEvent @event, IDiagnosticEventSink sink, Exception ex)
        {
            try
            {
                var handlers = Exception;
                if (handlers == null) return;

                var args = new DiagnosticSinkExceptionEventArgs(@event, sink, ex);
                handlers(this, args);
            }
            catch (Exception)
            {
                // Ignored
            }
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
