using System;
using Platibus.Diagnostics;
#if NET452
using System.IdentityModel.Tokens;
#endif
#if NETSTANDARD2_0
using Microsoft.IdentityModel.Tokens;
#endif

namespace Platibus.Security
{
    /// <summary>
    /// Options for configuring a <see cref="JwtSecurityTokenService"/>
    /// </summary>
    public class JwtSecurityTokenServiceOptions
    {
        /// <summary>
        /// The key used to sign new tokens and the primary key used to
        /// verify previously issued tokens
        /// </summary>
        public SecurityKey SigningKey { get; set; }

        /// <summary>
        /// The fallback key also used to verify previously issued tokens
        /// in cases in which validation fails using the primary
        /// <see cref="SigningKey"/>.
        /// </summary>
        /// <remarks>
        /// This is used to rotate keys.  During key rotation, the current
        /// <see cref="SigningKey"/> becomes the fallback signing key and a
        /// new signing key is generated.
        /// </remarks>
        public SecurityKey FallbackSigningKey { get; set; }

        /// <summary>
        /// The diagnostics service through which events related to issuing
        /// and validating tokens will be emitted
        /// </summary>
        public IDiagnosticService DiagnosticService { get; set; }

        /// <summary>
        /// The default Time To Live (TTL) for issued token in which no
        /// explicit expiration is given.
        /// </summary>
        public TimeSpan DefaultTTL { get; set; }

        public JwtSecurityTokenServiceOptions()
        {
            DiagnosticService = Diagnostics.DiagnosticService.DefaultInstance;
            DefaultTTL = TimeSpan.FromDays(3);
        }
    }
}
