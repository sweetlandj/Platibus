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

namespace Platibus.Diagnostics
{
    /// <summary>
    /// Discrete levels of diagnostic events that can assist with handling logic in 
    /// <see cref="IDiagnosticService"/> implementations
    /// </summary>
    public enum DiagnosticEventLevel
    {
        /// <summary>
        /// Routine low-level events used for monitoring or metrics gathering
        /// </summary>
        Trace = -1,

        /// <summary>
        /// Routine low-level events exposing intermediate state, branch logic, or other 
        /// information useful for troubleshooting unexpected conditions or outcomes
        /// </summary>
        Debug = 0,

        /// <summary>
        /// Routine high-level events used to verify correct operation
        /// </summary>
        Info = 1,

        /// <summary>
        /// Unexpected but recoverable conditions that may warrant attention
        /// </summary>
        Warn = 2,

        /// <summary>
        /// Unexpected and unrecoverable conditions that may require review and intervention
        /// </summary>
        Error = 3
    }
}
