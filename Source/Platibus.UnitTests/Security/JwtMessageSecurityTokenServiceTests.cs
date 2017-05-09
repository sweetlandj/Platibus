using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using NUnit.Framework;
using Platibus.Security;

namespace Platibus.UnitTests.Security
{
    public class JwtMessageSecurityTokenServiceTests
    {
        private static readonly RNGCryptoServiceProvider RNG = new RNGCryptoServiceProvider();

        protected JwtMessageSecurityTokenService MessageSecurityTokenService;
        protected ClaimsPrincipal Principal;
        protected DateTime? Expiration;
        protected string IssuedToken;

        [Test]
        public async Task SignedMessageSecurityTokenCanBeValidated()
        {
            GivenJwtMessageSecurityTokenServiceWithSigningKey();
            GivenClaimsPrincipal();
            await WhenATokenIsIssued();
            // Output for testing/verification on jwt.io
            Console.Out.WriteLine("Authorization: Bearer " + IssuedToken);
            await AssertIssuedTokenIsValid();
        }

        [Test]
        public async Task UnsignedMessageSecurityTokenCanBeValidated()
        {
            GivenJwtMessageSecurityTokenServiceWithNoSigningKey();
            GivenClaimsPrincipal();
            await WhenATokenIsIssued();
            // Output for testing/verification on jwt.io
            Console.Out.WriteLine("Authorization: Bearer " + IssuedToken);
            await AssertIssuedTokenIsValid();
        }

        protected SecurityKey GenerateSecurityKey()
        {
            var signingKeyBytes = new byte[16];
            RNG.GetBytes(signingKeyBytes);
            // Output for testing/verification on jwt.io
            Console.WriteLine(Convert.ToBase64String(signingKeyBytes));
            return new InMemorySymmetricSecurityKey(signingKeyBytes);
        }

        protected void GivenJwtMessageSecurityTokenServiceWithSigningKey()
        {
            var signingKey = GenerateSecurityKey();
            MessageSecurityTokenService = new JwtMessageSecurityTokenService(signingKey);
        }

        protected void GivenJwtMessageSecurityTokenServiceWithNoSigningKey()
        {
            MessageSecurityTokenService = new JwtMessageSecurityTokenService();
        }

        protected void GivenClaimsPrincipal()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "test@example.com"),
                new Claim(ClaimTypes.NameIdentifier, "test@example.com"),
                new Claim(ClaimTypes.Expiration, DateTime.UtcNow.AddMinutes(5).ToString("O")),
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
            IssuedToken = await MessageSecurityTokenService.Issue(Principal, Expiration);
        }

        protected async Task AssertIssuedTokenIsValid()
        {
            if (string.IsNullOrWhiteSpace(IssuedToken)) Assert.Inconclusive();
            var validatedPrincipal = await MessageSecurityTokenService.Validate(IssuedToken);
            Assert.NotNull(validatedPrincipal);

            Assert.True(validatedPrincipal.IsInRole("test"));
            Assert.True(validatedPrincipal.IsInRole("example"));

            var validatedClaimsPrincipal = validatedPrincipal as ClaimsPrincipal;
            Assert.NotNull(validatedClaimsPrincipal);

            var validatedIdentity = validatedClaimsPrincipal.Identity;
            Assert.NotNull(validatedIdentity);
            Assert.AreEqual(Principal.Identity.Name, validatedIdentity.Name);

            foreach (var claim in Principal.Claims)
            {
                Assert.True(validatedClaimsPrincipal.HasClaim(claim.Type, claim.Value));
            }
        }
    }
}
