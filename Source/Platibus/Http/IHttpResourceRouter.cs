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
using System.Threading.Tasks;

namespace Platibus.Http
{
    /// <summary>
    /// An interface that describes an object that directs an <see cref="IHttpResourceRequest"/>
    /// to the appropriate controller or handler
    /// </summary>
    /// <seealso cref="IHttpResourceController"/>
    public interface IHttpResourceRouter
    {
        /// <summary>
        /// Indicates whether the URI contains a path supported by this router
        /// </summary>
        /// <param name="uri">The URI in question</param>
        /// <returns>Returns <c>true</c> if this resource can be routed; <c>false</c> otherwise</returns>
        bool IsRoutable(Uri uri);

        /// <summary>
        /// Routes a <paramref name="request"/> and <paramref name="response"/> to
        /// the appropriate controller
        /// </summary>
        /// <param name="request">The request to route</param>
        /// <param name="response">The response to route</param>
        /// <returns>
        /// Returns a task that completes once the request has been routed and handled
        /// </returns>
        Task Route(IHttpResourceRequest request, IHttpResourceResponse response);
    }
}