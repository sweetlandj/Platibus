// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
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

namespace Platibus.Utils
{
    /// <summary>
    /// Extension methods for working with wait handles in an async context
    /// </summary>
    public static class WaitHandleExtensions
    {
        /// <summary>
        /// Returns a task that will complete when the specified 
        /// <paramref name="waitHandle"/> is set or the specified 
        /// <paramref name="timeout"/> is reached.
        /// </summary>
        /// <param name="waitHandle">The wait handle</param>
        /// <param name="timeout">The maximum amount of time to wait
        /// for the wait handle to be set</param>
        /// <returns>
        /// Returns a task whose result will be <c>true</c> if the wait 
        /// handle is set or <c>false</c> if the wait times out
        /// </returns>
        public static Task<bool> WaitOneAsync(this WaitHandle waitHandle, TimeSpan timeout = default(TimeSpan))
        {
            var tcs = new TaskCompletionSource<bool>(false);

            ThreadPool.RegisterWaitForSingleObject(waitHandle, (o, timedOut) => { tcs.SetResult(!timedOut); }, null,
                timeout, true);

            return tcs.Task;
        }
    }
}