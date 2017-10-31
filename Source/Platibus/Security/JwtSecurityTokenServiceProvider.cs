using System.Threading.Tasks;
using Platibus.Config;
using Platibus.Config.Extensibility;

namespace Platibus.Security
{
    /// <inheritdoc />
    /// <summary>
    /// Provides implementation of <see cref="T:Platibus.Security.ISecurityTokenService" /> based on 
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
            var defaultTtl = configuration.GetTimeSpan("defaultTtl");
            var options = new JwtSecurityTokenServiceOptions
            {
                SigningKey = signingKey,
                FallbackSigningKey = fallbackSigningKey,
                DefaultTTL = defaultTtl
            };
            var securityTokenService = new JwtSecurityTokenService(options);
            return Task.FromResult<ISecurityTokenService>(securityTokenService);
        }
    }
}
