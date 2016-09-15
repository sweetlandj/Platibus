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

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Platibus.Http
{
    /// <summary>
    /// An object that handles a request involving an HTTP resource
    /// </summary>
    public interface IHttpResourceController
    {
        /// <summary>
        /// Processes the specified <paramref name="request"/> and updates the supplied
        /// <paramref name="response"/>
        /// </summary>
        /// <param name="request">The HTTP resource request to process</param>
        /// <param name="response">The HTTP response to update</param>
        /// <param name="subPath">The portion of the request path that remains after the
        /// request was routed to this controller</param>
        /// <returns>Returns a task that completes when the request has been processed and the
        /// response has been updated</returns>
        Task Process(IHttpResourceRequest request, IHttpResourceResponse response, IEnumerable<string> subPath);
    }
}