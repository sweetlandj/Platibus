using System;
using System.Collections.Generic;
using System.Security.Claims;
using NUnit.Framework;
using Platibus.Security;

namespace Platibus.UnitTests.Security
{
    public class MessageSecurityTokenTests
    {
        [Test]
        public void ClaimsSurviveRoundTrip()
        {
            var originalClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "test@example.com"),
                new Claim(ClaimTypes.Role, "test"),
                new Claim(ClaimTypes.Role, "example"),
                new Claim("custom_claim1", Guid.NewGuid().ToString()),
                new Claim("custom_claim2", Guid.NewGuid().ToString())
            };
            var originalIdentity = new ClaimsIdentity(originalClaims);
            var originalPrincipal = new ClaimsPrincipal(originalIdentity);
            var messageSecurityToken = MessageSecurityToken.Create(originalPrincipal);
            var roundTripPrincipal = MessageSecurityToken.Validate(messageSecurityToken);
            Assert.NotNull(roundTripPrincipal);

            var roundTripIdentity = roundTripPrincipal.Identity;
            Assert.NotNull(roundTripIdentity);
            Assert.IsInstanceOf<ClaimsIdentity>(roundTripIdentity);
            Assert.AreEqual(roundTripIdentity.Name, originalIdentity.Name);

            var roundTripClaimsIdentity = roundTripIdentity as ClaimsIdentity;
            if (roundTripClaimsIdentity == null) Assert.Inconclusive();

            foreach (var claim in originalClaims)
            {
                Assert.That(roundTripClaimsIdentity.HasClaim(claim.Type, claim.Value));
            }

            Assert.That(roundTripPrincipal.IsInRole("test"));
            Assert.That(roundTripPrincipal.IsInRole("example"));
        }
    }
}
