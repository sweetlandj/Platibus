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
            endpoint = _endpoints.Values.FirstOrDefault(e => Equals(address, e.Address));
            return endpoint != null;
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
