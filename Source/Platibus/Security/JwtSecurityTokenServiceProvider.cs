using System.Threading.Tasks;
using Platibus.Config;
using Platibus.Config.Extensibility;

namespace Platibus.Security
{
    [Provider("JWT")]
    public class JwtSecurityTokenServiceProvider : ISecurityTokenServiceProvider
    {
        public Task<ISecurityTokenService> CreateSecurityTokenService(SecurityTokensElement configuration)
        {
            var signingKey = (HexEncodedSecurityKey)configuration.GetString("signingKey");
            var fallbackSigningKey = (HexEncodedSecurityKey)configuration.GetString("fallbackSigningKey");
            var securityTokenService = new JwtSecurityTokenService(signingKey, fallbackSigningKey);
            return Task.FromResult<ISecurityTokenService>(securityTokenService);
        }
    }
}
