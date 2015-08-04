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