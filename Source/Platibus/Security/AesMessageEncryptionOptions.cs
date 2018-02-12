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

#if NET452 || NET461
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
        public IDiagnosticService DiagnosticService { get; set; }

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
        /// <param name="key">The primary key used to encrypt and sign messages and the 
        /// first key used when decrypting messages and verifying signatures</param>
        public AesMessageEncryptionOptions(SymmetricSecurityKey key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }
    }
}
