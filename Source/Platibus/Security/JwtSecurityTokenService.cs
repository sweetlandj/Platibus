// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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
    /// An implementation of the <see cref="ISecurityTokenService"/> based on JSON Web
    /// Tokens (JWT)
    /// </summary>
    public class JwtSecurityTokenService : ISecurityTokenService
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime MaxExpires = UnixEpoch.AddSeconds(int.MaxValue);

        private readonly SecurityKey _signingKey;
        private readonly SecurityKey _fallbackSigningKey;
        private readonly string _signingAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256";
        private readonly string _digestAlgorithm = "http://www.w3.org/2001/04/xmlenc#sha256";
        
        /// <summary>
        /// Initializes a new <see cref="JwtSecurityTokenService"/>
        /// </summary>
        /// <param name="signingKey">(Optional) The key used to sign and verify tokens</param>
        /// <param name="fallbackSigningKey">(Optional) A fallback key used to verify previously
        /// issued tokens.  (Used for key rotation.)</param>
        public JwtSecurityTokenService(SecurityKey signingKey = null, SecurityKey fallbackSigningKey = null)
        {
            _signingKey = signingKey;
            _fallbackSigningKey = fallbackSigningKey;
        }

        /// <inheritdoc />
        public Task<string> Issue(IPrincipal principal, DateTime? expires = null)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }

            var myExpires = expires > MaxExpires ? null : expires;
            var identity = principal.Identity;
            var claimsIdentity = identity as ClaimsIdentity ?? new ClaimsIdentity(identity);
            var lifetime = new Lifetime(DateTime.UtcNow, myExpires);
            
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
        public Task<IPrincipal> Validate(string securityToken)
        {
            if (string.IsNullOrWhiteSpace(securityToken))
            {
                throw new ArgumentNullException("securityToken");
            }

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
            var principal = tokenHandler.ValidateToken(securityToken, parameters, out token);
            return Task.FromResult<IPrincipal>(principal);
        }
    }
}
