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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Platibus.Diagnostics;
using Microsoft.IdentityModel.Tokens;

namespace Platibus.Security
{
    /// <inheritdoc />
    /// <summary>
    /// An implementation of the <see cref="T:Platibus.Security.ISecurityTokenService" /> based on JSON Web
    /// Tokens (JWT)
    /// </summary>
    public class JwtSecurityTokenService : ISecurityTokenService
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime MaxExpires = UnixEpoch.AddSeconds(int.MaxValue);

        private readonly SecurityKey _signingKey;
        private readonly SecurityKey _fallbackSigningKey;
        private readonly string _signingAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256";
        private readonly IDiagnosticService _diagnosticService;
        private readonly TimeSpan _defaultTtl;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.Security.JwtSecurityTokenService" /> that issues unsigned tokens
        /// </summary>
        public JwtSecurityTokenService() : this (new JwtSecurityTokenServiceOptions())
        {
        }

        /// <summary>
        /// Initializes a new <see cref="JwtSecurityTokenService"/> with the specified options
        /// </summary>
        public JwtSecurityTokenService(JwtSecurityTokenServiceOptions options)
        {
            var myOptions = options ?? new JwtSecurityTokenServiceOptions();
            _diagnosticService = myOptions.DiagnosticService ?? DiagnosticService.DefaultInstance;
            _signingKey = myOptions.SigningKey;
            _fallbackSigningKey = myOptions.FallbackSigningKey;
            _defaultTtl = myOptions.DefaultTTL <= TimeSpan.Zero 
                ? TimeSpan.FromDays(3) 
                : myOptions.DefaultTTL;
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.Security.JwtSecurityTokenService" />
        /// </summary>
        /// <param name="signingKey">(Optional) The key used to sign and verify tokens</param>
        /// <param name="fallbackSigningKey">(Optional) A fallback key used to verify previously
        /// issued tokens.  (Used for key rotation.)</param>
        /// <see cref="JwtSecurityTokenService(JwtSecurityTokenServiceOptions)"/>
        [Obsolete("Use JwtSecurityTokenService(JwtSecurityTokenServiceOptions)")]
        public JwtSecurityTokenService(SecurityKey signingKey = null, SecurityKey fallbackSigningKey = null)
            : this(new JwtSecurityTokenServiceOptions { SigningKey = signingKey, FallbackSigningKey = fallbackSigningKey})
        {
        }

        /// <inheritdoc />
        public Task<string> Issue(IPrincipal principal, DateTime? expires = null)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var myExpires = expires ?? DateTime.UtcNow.Add(_defaultTtl);
            if (myExpires > MaxExpires)
            {
                myExpires = MaxExpires;
            }

            var identity = principal.Identity;
            var claimsIdentity = identity as ClaimsIdentity ?? new ClaimsIdentity(identity);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = myExpires
            };

            if (_signingKey != null)
            {
                var signingCredentials = new SigningCredentials(_signingKey, _signingAlgorithm);
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
                throw new ArgumentNullException(nameof(securityToken));
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

            IPrincipal principal;
            try
            {
                principal = tokenHandler.ValidateToken(securityToken, parameters, out SecurityToken _);
            }
            catch (Exception ex)
            {
                principal = null;
                _diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.InvalidSecurityToken)
                {
                    Exception = ex
                }.Build());
            }
            return Task.FromResult(principal);
        }
    }
}
