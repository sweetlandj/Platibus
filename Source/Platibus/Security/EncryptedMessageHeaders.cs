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
        public string Ciphertext
        {
            get => this[EncryptedHeaderName.Ciphertext];
            set => this[EncryptedHeaderName.Ciphertext] = value;
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
