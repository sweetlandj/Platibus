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

using Platibus.Diagnostics;

namespace Platibus.SQL
{
    /// <summary>
    /// Diagnostic event types related to SQL events
    /// </summary>
    public static class SQLEventType
    {
        /// <summary>
        /// Emitted when a connection is opened
        /// </summary>
        public static readonly DiagnosticEventType ConnectionOpened = new DiagnosticEventType("ConnectionOpened", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted when a connection is closed
        /// </summary>
        public static readonly DiagnosticEventType ConnectionClosed = new DiagnosticEventType("ConnectionClosed", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted when there is an error opening or closing a connection
        /// </summary>
        public static readonly DiagnosticEventType ConnectionError = new DiagnosticEventType("ConnectionError", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted when there is an error opening or closing a connection
        /// </summary>
        public static readonly DiagnosticEventType CommandExecuted = new DiagnosticEventType("CommandExecuted", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted when there is an error opening or closing a connection
        /// </summary>
        public static readonly DiagnosticEventType CommandError = new DiagnosticEventType("CommandError", DiagnosticEventLevel.Error);

        /// <summary>
        /// Emitted when message headers or content cannot be read from the database
        /// </summary>
        public static readonly DiagnosticEventType MessageRecordFormatError = new DiagnosticEventType("MessageFormatError", DiagnosticEventLevel.Error);

    }
}
