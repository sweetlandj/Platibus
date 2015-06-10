using System;
using System.Collections.Generic;

namespace Platibus
{
    public interface IEndpointCollection : IEnumerable<KeyValuePair<EndpointName, IEndpoint>>
    {
        IEndpoint this[EndpointName endpointName] { get; }
        bool TryGetEndpointByUri(Uri uri, out IEndpoint endpoint);
    }
}