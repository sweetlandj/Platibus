using System.Threading.Tasks;
using Platibus.Config;
using Platibus.Config.Extensibility;

namespace Platibus.Security
{
    /// <summary>
    /// Provides implementation of <see cref="ISecurityTokenService"/> based on 
    /// JSON Web Tokens (JWT)
    /// </summary>
    [Provider("JWT")]
    public class JwtSecurityTokenServiceProvider : ISecurityTokenServiceProvider
    {
        /// <inheritdoc />
        public Task<ISecurityTokenService> CreateSecurityTokenService(SecurityTokensElement configuration)
        {
            var signingKey = (HexEncodedSecurityKey)configuration.GetString("signingKey");
            var fallbackSigningKey = (HexEncodedSecurityKey)configuration.GetString("fallbackSigningKey");
            var securityTokenService = new JwtSecurityTokenService(signingKey, fallbackSigningKey);
            return Task.FromResult<ISecurityTokenService>(securityTokenService);
        }
    }
}
