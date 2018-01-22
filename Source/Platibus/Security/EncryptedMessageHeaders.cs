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

using System.Collections.Generic;

namespace Platibus.Security
{
    /// <inheritdoc />
    /// <summary>
    /// Encrypted messages headers
    /// </summary>
    public class EncryptedMessageHeaders : MessageHeaders
    {
        /// <summary>
        /// The base-64 encoded initialization vector used to encrypt the message
        /// </summary>
        public string IV  
        {
            get => this[EncryptedHeaderName.IV];
            set => this[EncryptedHeaderName.IV] = value;
        }

        /// <summary>
        /// The base-64 encoded encrypted headers
        /// </summary>
        public string Headers
        {
            get => this[EncryptedHeaderName.Headers];
            set => this[EncryptedHeaderName.Headers] = value;
        }

        /// <summary>
        /// The base-64 encoded signature
        /// </summary>
        public string Signature
        {
            get => this[EncryptedHeaderName.Signature];
            set => this[EncryptedHeaderName.Signature] = value;
        }

        /// <summary>
        /// The signature algorithm
        /// </summary>
        public string SignatureAlgorithm
        {
            get => this[EncryptedHeaderName.SignatureAlgorithm];
            set => this[EncryptedHeaderName.SignatureAlgorithm] = value;
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes an empty <see cref="T:Platibus.Security.EncryptedMessageHeaders" /> instance
        /// </summary>
        public EncryptedMessageHeaders()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a <see cref="T:Platibus.Security.EncryptedMessageHeaders" /> instance
        /// with the specified header values
        /// </summary>
        /// <param name="headers">The initial header values</param>
        public EncryptedMessageHeaders(IEnumerable<KeyValuePair<HeaderName, string>> headers) : base(headers)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a <see cref="T:Platibus.Security.EncryptedMessageHeaders" /> instance
        /// with the specified header values
        /// </summary>
        /// <param name="headers">The initial header values</param>
        public EncryptedMessageHeaders(IEnumerable<KeyValuePair<string, string>> headers) : base(headers)
        {
        }
    }
}
