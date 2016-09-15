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
using System.Collections.Generic;

namespace Platibus
{
    /// <summary>
    /// A collection of named endpoints used to centralize and share endpoint
    /// configuration across multiple components
    /// </summary>
    public interface IEndpointCollection : IEnumerable<KeyValuePair<EndpointName, IEndpoint>>
    {
        /// <summary>
        /// Returns the endpoint with the specified name
        /// </summary>
        /// <param name="endpointName">The name of the endpoint</param>
        /// <returns>Returns the endpoint</returns>
        /// <exception cref="EndpointNotFoundException">Thrown if there
        /// is no endpoint with the specified name</exception>
        IEndpoint this[EndpointName endpointName] { get; }

        /// <summary>
        /// Tries to retrieve the endpoint with the specified address
        /// </summary>
        /// <param name="address">The address of the endpoint</param>
        /// <param name="endpoint">An output parameter that will be initialied
        /// with the endpoint if the endpoint is found</param>
        /// <returns>Returns <c>true</c> if the endpoint is found; <c>false</c>
        /// otherwise</returns>
        bool TryGetEndpointByAddress(Uri address, out IEndpoint endpoint);
    }
}