using System.Security.Claims;
using System.Security.Principal;

namespace Platibus.Security
{
    /// <summary>
    /// Helper methods for working with <see cref="System.Security.Principal.IPrincipal"/>
    /// implementations
    /// </summary>
    public static class PrincipalExtensions
    {
        /// <summary>
        /// Returns a claim from the principal
        /// </summary>
        /// <param name="principal">The principal</param>
        /// <param name="claimType">The type of claim</param>
        /// <returns>The value of the specified claim as a string if present; <c>null</c>
        /// otherwise</returns>
        public static string GetClaimValue(this IPrincipal principal, string claimType)
        {
            if (principal == null) return null;
            var claimsIdentity = principal.Identity as ClaimsIdentity;
            if (claimsIdentity == null) return null;

            var claim = claimsIdentity.FindFirst(claimType);
            return claim == null ? null : claim.Value;
        }
    }
}
