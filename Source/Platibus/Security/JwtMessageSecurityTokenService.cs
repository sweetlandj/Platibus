using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Platibus.Security
{
    /// <summary>
    /// An implementation of the <see cref="IMessageSecurityTokenService"/> based on JSON Web
    /// Tokens (JWT)
    /// </summary>
    public class JwtMessageSecurityTokenService : IMessageSecurityTokenService
    {
        private readonly SecurityKey _signingKey;
        private readonly SecurityKey _fallbackSigningKey;
        private readonly string _signingAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256";
        private readonly string _digestAlgorithm = "http://www.w3.org/2001/04/xmlenc#sha256";
        
        /// <summary>
        /// Initializes a new <see cref="JwtMessageSecurityTokenService"/>
        /// </summary>
        /// <param name="signingKey">(Optional) The key used to sign and verify tokens</param>
        /// <param name="fallbackSigningKey">(Optional) A fallback key used to verify previously
        /// issued tokens.  (Used for key rotation.)</param>
        public JwtMessageSecurityTokenService(SecurityKey signingKey = null, SecurityKey fallbackSigningKey = null)
        {
            _signingKey = signingKey;
            _fallbackSigningKey = fallbackSigningKey;
        }

        /// <inheritdoc />
        public Task<string> Issue(IPrincipal principal, DateTime? expires = null)
        {
            if (principal == null) throw new ArgumentNullException("principal");

            var identity = principal.Identity;
            var claimsIdentity = identity as ClaimsIdentity ?? new ClaimsIdentity(identity);
            var lifetime = new Lifetime(DateTime.UtcNow, expires);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Lifetime = lifetime
            };

            if (_signingKey != null)
            {
                var signingCredentials = new SigningCredentials(_signingKey, _signingAlgorithm, _digestAlgorithm);
                tokenDescriptor.SigningCredentials = signingCredentials;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Task.FromResult(tokenHandler.WriteToken(token));
        }

        /// <inheritdoc />
        public Task<IPrincipal> Validate(string messageSecurityToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var signingKeys = new List<SecurityKey>();
            if (_signingKey != null)
            {
                signingKeys.Add(_signingKey);
            }

            if (_fallbackSigningKey != null)
            {
                signingKeys.Add(_fallbackSigningKey);
            }

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false
            };

            if (signingKeys.Any())
            {
                parameters.IssuerSigningKeys = signingKeys;
                parameters.RequireSignedTokens = true;
            }
            else
            {
                parameters.RequireSignedTokens = false;
            }
            
            SecurityToken token;
            var principal = tokenHandler.ValidateToken(messageSecurityToken, parameters, out token);
            return Task.FromResult<IPrincipal>(principal);
        }
    }
}
