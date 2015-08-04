using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Platibus.Config;

namespace Platibus
{
    public class EndpointCollection : IEndpointCollection
    {
        private readonly IDictionary<EndpointName, IEndpoint> _endpoints = new Dictionary<EndpointName, IEndpoint>();

        public void Add(EndpointName endpointName, IEndpoint endpoint)
        {
            if (endpointName == null) throw new ArgumentNullException("endpointName");
            if (endpoint == null) throw new ArgumentNullException("endpoint");
            if (_endpoints.ContainsKey(endpointName)) throw new EndpointAlreadyExistsException(endpointName);
            _endpoints[endpointName] = endpoint;
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

        public bool TryGetEndpointByAddress(Uri address, out IEndpoint endpoint)
        {
            endpoint = _endpoints.Values.FirstOrDefault(e => Equals(address, e.Address));
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
