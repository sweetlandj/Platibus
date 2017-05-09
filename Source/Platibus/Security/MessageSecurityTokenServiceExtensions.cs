using System;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Platibus.Security
{
    /// <summary>
    /// Extension methods to make working with <see cref="IMessageSecurityTokenService"/>
    /// implementations safer and more convenient
    /// </summary>
    public static class MessageSecurityTokenServiceExtensions
    {
        /// <summary>
        /// Validates the specified <paramref name="token"/>, returning <c>null</c> if the token is
        /// null or whitespace
        /// </summary>
        /// <param name="service">The message security token service used to validate the token</param>
        /// <param name="token">The token to validate</param>
        /// <returns>Returns the validated <see cref="IPrincipal"/> if <see cref="token"/> is not
        /// null or whitespace; returns <c>null</c> otherwise</returns>
        public static async Task<IPrincipal> NullSafeValidate(this IMessageSecurityTokenService service, string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;
            return await service.Validate(token);
        }

        /// <summary>
        /// Issues a security token for the specified <paramref name="principal"/>, returning 
        /// <c>null</c> if the principal is null
        /// </summary>
        /// <param name="service">The message security token service used to validate the token</param>
        /// <param name="principal">The principal for which a security token is to be issued.  Can
        /// be <c>null</c>.</param>
        /// <param name="expires">(Optional) The date/time at which the issued token should expire</param>
        /// <returns>Returns a security token for the specified <see cref="principal"/> if it is
        /// not null; returns <c>null</c> otherwise</returns>
        public static async Task<string> NullSafeIssue(this IMessageSecurityTokenService service, IPrincipal principal, DateTime? expires = null)
        {
            if (principal == null) return null;
            return await service.Issue(principal, expires);
        }
    }
}
