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
#if NETSTANDARD2_0 || NET461
            return Task.CompletedTask;
#endif
#if NET452
            return Task.FromResult(false);
#endif
        }
    }
}
