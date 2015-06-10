using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Platibus
{
    public class ReadOnlyEndpointCollection : IEndpointCollection
    {
        public static ReadOnlyEndpointCollection Empty = new ReadOnlyEndpointCollection();

        private readonly IDictionary<EndpointName, IEndpoint> _endpoints;

        private ReadOnlyEndpointCollection()
        {
            _endpoints = new Dictionary<EndpointName, IEndpoint>();
        }

        public ReadOnlyEndpointCollection(IEndpointCollection endpointCollection)
        {
            if (endpointCollection == null) throw new ArgumentNullException("endpointCollection");
            _endpoints = endpointCollection.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

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

        public bool TryGetEndpointByUri(Uri uri, out IEndpoint endpoint)
        {
            endpoint = _endpoints.Values.FirstOrDefault(e => Equals(uri, e.Address));
            return endpoint != null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<EndpointName, IEndpoint>> GetEnumerator()
        {
            return _endpoints.GetEnumerator();
        }
    }
}