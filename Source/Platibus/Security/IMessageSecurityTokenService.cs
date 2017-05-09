using System;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Platibus.Security
{
    /// <summary>
    /// An interface describing an object that can generate a security token for a 
    /// <see cref="System.Security.Principal.IPrincipal"/> or validate a previously generated
    /// security token.
    /// </summary>
    public interface IMessageSecurityTokenService
    {
        /// <summary>
        /// Issues a new security token representing the specified
        /// <paramref name="principal"/>
        /// </summary>
        /// <param name="principal">The principal</param>
        /// <param name="expires">(Optional) The date/time at which the token should expire</param>
        /// <returns>Returns a task whose result is a message security token representing the
        /// specified <paramref name="principal"/></returns>
        Task<string> Issue(IPrincipal principal, DateTime? expires = null);

        /// <summary>
        /// Validates a previously issued security token
        /// </summary>
        /// <param name="messageSecurityToken">The message security token</param>
        /// <returns>Returns the principal represented b the specified 
        /// <paramref name="messageSecurityToken"/></returns>
        Task<IPrincipal> Validate(string messageSecurityToken);
    }
}
