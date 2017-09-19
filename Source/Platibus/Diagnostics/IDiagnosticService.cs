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

using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// Interface through which Platibus reports diagnostic events
    /// </summary>
    /// <remarks>
    /// Implementations of this interface must be threadsafe.
    /// </remarks>
    public interface IDiagnosticService
    {
        /// <summary>
        /// Event raised when an unhandled exception is caught from one of the registered sinks
        /// </summary>
        event DiagnosticSinkExceptionHandler Exception;

        /// <summary>
        /// Registers a data sink to receive a copy of each diagnostic event that is emitted
        /// </summary>
        /// <param name="sink">The sink to add</param>
        void AddSink(IDiagnosticEventSink sink);

        /// <summary>
        /// Deregisters a previously registered data sink
        /// </summary>
        /// <param name="sink">The sink to remove</param>
        void RemoveSink(IDiagnosticEventSink sink);

        /// <summary>
        ///  Receives and handles a diagnostic event
        /// </summary>
        /// <param name="event">The diagnostic event to consume</param>
        void Emit(DiagnosticEvent @event);

        /// <summary>
        /// Receives and handles a diagnostic event asynchronously
        /// </summary>
        /// <param name="event">The diagnostic event to consume</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be provided
        /// by the caller to interrupt processing of the diagnostic event</param>
        Task EmitAsync(DiagnosticEvent @event, CancellationToken cancellationToken = default(CancellationToken));
    }
}
