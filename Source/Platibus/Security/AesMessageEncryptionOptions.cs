#if NET452
using System.IdentityModel.Tokens;
#endif
#if NETSTANDARD2_0
using Microsoft.IdentityModel.Tokens;
#endif
using Platibus.Diagnostics;
using System;
using System.Collections.Generic;

namespace Platibus.Security
{
    /// <summary>
    /// Options that influence the behavior of the <see cref="AesMessageEncryptionService"/>
    /// </summary>
    public class AesMessageEncryptionOptions
    {
        /// <summary>
        /// The diagnostic service through which diagnostic events will be raised
        /// </summary>
        public IDiagnosticService DiagnosticService { get; }

        /// <summary>
        /// The primary key used to encrypt and sign messages and the first key used
        /// when decrypting messages and verifying signatures
        /// </summary>
        public SymmetricSecurityKey Key { get; }

        /// <summary>
        /// Alternate keys to try when decrypting and verifying messages in the event
        /// that the primary <see cref="Key"/> fails.
        /// </summary>
        /// <remarks>
        /// Fallback keys are useful for key rotation.  For example, a replacement key
        /// can first be released to all nodes as a fallback key to all nodes.  Then
        /// the replacement key can be swapped with the existing <see cref="Key"/> to
        /// begin encrypting new messages with the new key.  Meanwhile, other nodes
        /// are able to decrypt messages with the previous key until they are updated.
        /// </remarks>
        public IEnumerable<SymmetricSecurityKey> FallbackKeys { get; set; }

        /// <summary>
        /// Initializes a new set of <see cref="AesMessageEncryptionOptions"/>
        /// </summary>
        /// <param name="diagnosticService">The diagnostic service through which diagnostic 
        /// events will be raised</param>
        /// <param name="key">The primary key used to encrypt and sign messages and the 
        /// first key used when decrypting messages and verifying signatures</param>
        public AesMessageEncryptionOptions(IDiagnosticService diagnosticService, SymmetricSecurityKey key)
        {
            DiagnosticService = diagnosticService ?? throw new ArgumentNullException(nameof(diagnosticService));
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }
    }
}
