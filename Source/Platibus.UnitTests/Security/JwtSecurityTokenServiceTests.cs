// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
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
#if NET452
using System.IdentityModel.Tokens;
#endif
#if NETCOREAPP2_0
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
#endif
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading.Tasks;
using Xunit;
using Platibus.Security;

namespace Platibus.UnitTests.Security
{
    [Trait("Category", "UnitTests")]
    public class JwtSecurityTokenServiceTests
    {
        private static readonly RNGCryptoServiceProvider RNG = new RNGCryptoServiceProvider();

        protected JwtSecurityTokenServiceOptions Options = new JwtSecurityTokenServiceOptions();
        protected ClaimsPrincipal Principal;
        protected DateTime? Expiration;
        protected string IssuedToken;
        protected bool TokenValid;
        protected IPrincipal ValidatedPrincipal;
        protected SecurityToken SecurityToken;

        [Fact]
        public async Task SignedMessageSecurityTokenCanBeValidated()
        {
            GivenSigningKey();
            GivenClaimsPrincipal();
            await WhenATokenIsIssued();
            // Output for testing/verification on jwt.io
            Console.Out.WriteLine("Authorization: Bearer " + IssuedToken);
            AssertIssuedTokenIsValid();
        }

        [Fact]
        public async Task TokensExpireAfterDefaultTTL()
        {
            GivenSigningKey();
            GivenClaimsPrincipal();

            var ttl = TimeSpan.FromMinutes(5);
            GivenDefaultTTL(ttl);

            await WhenATokenIsIssued();
            // Output for testing/verification on jwt.io
            Console.Out.WriteLine("Authorization: Bearer " + IssuedToken);
            AssertExpiresAtOrAround(DateTime.UtcNow.Add(ttl));
        }

        [Fact]
        public async Task TokensExpireAtExplicitExpirationDate()
        {
            GivenSigningKey();
            GivenClaimsPrincipal();

            Expiration = DateTime.UtcNow.AddMinutes(5);

            await WhenATokenIsIssued();
            // Output for testing/verification on jwt.io
            Console.Out.WriteLine("Authorization: Bearer " + IssuedToken);

            AssertExpiresAtOrAround(Expiration.GetValueOrDefault());
        }

        [Fact]
        public async Task UnsignedMessageSecurityTokenCanBeValidated()
        {
            GivenNoSigningKey();
            GivenClaimsPrincipal();
            await WhenATokenIsIssued();
            // Output for testing/verification on jwt.io
            Console.Out.WriteLine("Authorization: Bearer " + IssuedToken);
            AssertIssuedTokenIsValid();
        }

        [Fact]
        public async Task MessageSecurityTokenSignedWithFallbackKeyCanBeValidated()
        {
            GivenSigningKey();
            GivenClaimsPrincipal();
            await WhenATokenIsIssued();
            WhenSigningKeyIsUpdated();
            // Output for testing/verification on jwt.io
            Console.Out.WriteLine("Authorization: Bearer " + IssuedToken);
            AssertIssuedTokenIsValid();
        }

#if NET452
        protected SymmetricSecurityKey GenerateSecurityKey()
        {
            var signingKeyBytes = new byte[16];
            RNG.GetBytes(signingKeyBytes);
            // Output for testing/verification on jwt.io
            Console.WriteLine(Convert.ToBase64String(signingKeyBytes));
            return new InMemorySymmetricSecurityKey(signingKeyBytes);
        }
#endif
#if NETCOREAPP2_0
        protected SymmetricSecurityKey GenerateSecurityKey()
        {
            var signingKeyBytes = new byte[16];
            RNG.GetBytes(signingKeyBytes);
            // Output for testing/verification on jwt.io
            Console.WriteLine(Convert.ToBase64String(signingKeyBytes));
            return new SymmetricSecurityKey(signingKeyBytes);
        }
#endif

        protected void GivenNoSigningKey()
        {
            Options.SigningKey = null;
            Options.FallbackSigningKey = null;
        }

        protected void GivenSigningKey()
        {
            Options.SigningKey = GenerateSecurityKey();
        }

        protected void WhenSigningKeyIsUpdated()
        {
            Options.FallbackSigningKey = Options.SigningKey;
            Options.SigningKey = GenerateSecurityKey();
        }

        protected void GivenDefaultTTL(TimeSpan ttl)
        {
            Options.DefaultTTL = ttl;
        }
        
        protected void GivenClaimsPrincipal()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "test@example.com"),
                new Claim(ClaimTypes.NameIdentifier, "test@example.com"),
                new Claim(ClaimTypes.Role, "test"),
                new Claim(ClaimTypes.Role, "example"),
                new Claim("custom_claim1", Guid.NewGuid().ToString()),
                new Claim("custom_claim2", Guid.NewGuid().ToString())
            };
            var identity = new ClaimsIdentity(claims);
            Principal = new ClaimsPrincipal(identity);
        }

        protected async Task WhenATokenIsIssued()
        {
            var securityTokenService = new JwtSecurityTokenService(Options);
            IssuedToken = await securityTokenService.Issue(Principal, Expiration);
            ValidateIssuedToken();
        }

        protected void AssertIssuedTokenIsValid()
        {
            Assert.NotNull(IssuedToken);
            Assert.True(TokenValid);
            Assert.NotNull(ValidatedPrincipal);

            Assert.True(ValidatedPrincipal.IsInRole("test"));
            Assert.True(ValidatedPrincipal.IsInRole("example"));

            var validatedClaimsPrincipal = ValidatedPrincipal as ClaimsPrincipal;
            Assert.NotNull(validatedClaimsPrincipal);

            var validatedIdentity = validatedClaimsPrincipal.Identity;
            Assert.NotNull(validatedIdentity);
            Assert.Equal(Principal.Identity.Name, validatedIdentity.Name);

            foreach (var claim in Principal.Claims)
            {
                Assert.True(validatedClaimsPrincipal.HasClaim(claim.Type, claim.Value));
            }

            if (Expiration != null)
            {
                Assert.True(validatedClaimsPrincipal.HasClaim(c => c.Type == ClaimTypes.Expiration));
            }
        }

        protected void AssertExpiresAtOrAround(DateTime date)
        {
            AssertExpiresBetween(date.AddMinutes(-1), date.AddMinutes(1));
        }

        protected void AssertExpiresBetween(DateTime start, DateTime end)
        {
            Assert.NotNull(SecurityToken);
            Assert.True(start <= SecurityToken.ValidTo);
            Assert.True(end >= SecurityToken.ValidTo);
        }

        private void ValidateIssuedToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var signingKeys = new List<SecurityKey>();
            if (Options.SigningKey != null)
            {
                signingKeys.Add(Options.SigningKey);
            }

            if (Options.FallbackSigningKey != null)
            {
                signingKeys.Add(Options.FallbackSigningKey);
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

            try
            {
                ValidatedPrincipal = tokenHandler.ValidateToken(IssuedToken, parameters, out SecurityToken);
                TokenValid = true;
            }
            catch (Exception)
            {
                TokenValid = false;
            }
        }
    }
}
