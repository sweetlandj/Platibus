using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Http
{
    /// <summary>
    /// Provides access to pool HTTP client instances
    /// </summary>
    public interface IHttpClientFactory
    {
        /// <summary>
        /// Gets an HTTP client from the pool, creating a new HTTP client if necessary
        /// </summary>
        /// <param name="uri">The URI for the connection</param>
        /// <param name="credentials">(Optional) The credentials needed to make the connection</param>
        /// <param name="cancellationToken">(Optional) A token the can be used byy the caller
        /// to request cancellation of the aquisition attempt</param>
        /// <returns>
        /// Returns a task whose result is an HTTP client that can be used to connect to the
        /// specified <paramref name="uri"/> with the supplied <paramref name="credentials"/>
        /// </returns>
        Task<HttpClient> GetClient(Uri uri, IEndpointCredentials credentials, CancellationToken cancellationToken = default(CancellationToken));
    }
}