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

namespace Platibus.Diagnostics
{
    /// <summary>
    /// Arguments for events raised in response to unhandled exceptions thrown from 
    /// <see cref="IDiagnosticEventSink"/> implementations while handling events
    /// </summary>
    public class DiagnosticSinkExceptionEventArgs : EventArgs
    {
        private readonly DiagnosticEvent _diagnosticEvent;
        private readonly IDiagnosticEventSink _diagnosticEventSink;
        private readonly Exception _exception;

        /// <summary>
        /// The event being handled
        /// </summary>
        public DiagnosticEvent DiagnosticEvent { get { return _diagnosticEvent; } }

        /// <summary>
        /// The data sink from which the exception was thrown
        /// </summary>
        public IDiagnosticEventSink DiagnosticEventSink { get { return _diagnosticEventSink; } }
         
        /// <summary>
        /// The unhandled exception
        /// </summary>
        public Exception Exception { get { return _exception; } }

        /// <summary>
        /// Initializes a new <see cref="DiagnosticSinkExceptionEventArgs"/>
        /// </summary>
        /// <param name="diagnosticEvent">The event being handled</param>
        /// <param name="diagnosticEventSink">The data sink from which the exception was thrown</param>
        /// <param name="exception">The unhandled exception</param>
        public DiagnosticSinkExceptionEventArgs(DiagnosticEvent diagnosticEvent, IDiagnosticEventSink diagnosticEventSink, Exception exception)
        {
            _diagnosticEvent = diagnosticEvent;
            _diagnosticEventSink = diagnosticEventSink;
            _exception = exception;
        }
    }
}
