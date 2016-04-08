using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Platibus
{
    /// <summary>
    /// An immutable <see cref="IEndpointCollection"/>
    /// </summary>
    public class ReadOnlyEndpointCollection : IEndpointCollection
    {
        /// <summary>
        /// An empty and immutable endpoint collection singleton
        /// </summary>
        public static ReadOnlyEndpointCollection Empty = new ReadOnlyEndpointCollection();

        private readonly IDictionary<EndpointName, IEndpoint> _endpoints;

        private ReadOnlyEndpointCollection()
        {
            _endpoints = new Dictionary<EndpointName, IEndpoint>();
        }

        /// <summary>
        /// Initializes a new <see cref="ReadOnlyEndpointCollection"/> based on the
        /// endpoints in the specified <paramref name="endpointCollection"/>
        /// </summary>
        /// <param name="endpointCollection">An endpoint collection</param>
        public ReadOnlyEndpointCollection(IEndpointCollection endpointCollection)
        {
            if (endpointCollection == null) throw new ArgumentNullException("endpointCollection");
            _endpoints = endpointCollection.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
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

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
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