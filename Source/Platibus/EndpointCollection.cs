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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Platibus.Config;

namespace Platibus
{
    /// <summary>
    /// A mutable <see cref="IEndpointCollection"/> implementation
    /// </summary>
    public class EndpointCollection : IEndpointCollection
    {
        private readonly IDictionary<EndpointName, IEndpoint> _endpoints = new Dictionary<EndpointName, IEndpoint>();

        /// <summary>
        /// Adds a named endpoint to the collection
        /// </summary>
        /// <param name="endpointName">The name of the endpoint</param>
        /// <param name="endpoint">The endpoint</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="endpointName"/>
        /// or <paramref name="endpoint"/> are <c>null</c></exception>
        /// <exception cref="EndpointAlreadyExistsException">Thrown if there is already an
        /// endpoint with the specified <paramref name="endpointName"/></exception>
        public void Add(EndpointName endpointName, IEndpoint endpoint)
        {
            if (endpointName == null) throw new ArgumentNullException("endpointName");
            if (endpoint == null) throw new ArgumentNullException("endpoint");
            if (_endpoints.ContainsKey(endpointName)) throw new EndpointAlreadyExistsException(endpointName);
            _endpoints[endpointName] = endpoint;
        }

        /// <summary>
        /// Returns the endpoint with the specified name
        /// </summary>
        /// <param name="endpointName">The name of the endpoint</param>
        /// <returns>Returns the endpoint</returns>
        /// <exception cref="EndpointNotFoundException">Thrown if there
        /// is no endpoint with the specified name</exception>
        public IEndpoint this[EndpointName endpointName]
        {
            get
            {
                IEndpoint endpoint;
                if (!_endpoints.TryGetValue(endpointName, out endpoint))
                {
                    throw new EndpointNotFoundException(endpointName);
                }
                return endpoint;
            }
        }

        /// <summary>
        /// Tries to retrieve the endpoint with the specified address
        /// </summary>
        /// <param name="address">The address of the endpoint</param>
        /// <param name="endpoint">An output parameter that will be initialied
        /// with the endpoint if the endpoint is found</param>
        /// <returns>Returns <c>true</c> if the endpoint is found; <c>false</c>
        /// otherwise</returns>
        public bool TryGetEndpointByAddress(Uri address, out IEndpoint endpoint)
        {
            var comparer = new EndpointAddressEqualityComparer();
            endpoint = _endpoints.Values.FirstOrDefault(e => comparer.Equals(e.Address, address));
            return endpoint != null;
        }

        /// <inheritdoc />
        public bool Contains(EndpointName endpointName)
        {
            return _endpoints.ContainsKey(endpointName);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<EndpointName, IEndpoint>> GetEnumerator()
        {
            return _endpoints.GetEnumerator();
        }
    }
}
