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

namespace Platibus.Filesystem
{
    /// <summary>
    /// Types of <see cref="DiagnosticEvent"/>s specific to filesystem based services
    /// </summary>
    public static class FilesystemEventType
    {
        /// <summary>
        /// Emitted when a message file is created and written to disk
        /// </summary>
        public static readonly DiagnosticEventType MessageFileCreated = new DiagnosticEventType("MessageFileCreated", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted when a message file is deleted from disk
        /// </summary>
        public static readonly DiagnosticEventType MessageFileDeleted = new DiagnosticEventType("MessageFileDeleted", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted when a message file cannot be read from disk
        /// </summary>
        public static readonly DiagnosticEventType MessageFileFormatError = new DiagnosticEventType("MessageFileFormatError", DiagnosticEventLevel.Error);

    }
}
