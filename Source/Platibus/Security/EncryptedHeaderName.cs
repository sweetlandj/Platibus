namespace Platibus.Security
{
    /// <summary>
    /// Well-known header names for encrypted messages
    /// </summary>
    internal class EncryptedHeaderName
    {
        /// <summary>
        /// The initialization vector used to encrypt the message and its headers
        /// </summary>
        public static readonly HeaderName IV = "Platibus-IV";

        /// <summary>
        /// The encrypted headers
        /// </summary>
        public static readonly HeaderName Ciphertext = "Platibus-Ciphertext";

        /// <summary>
        /// The hash-based message authentication code used to verify the 
        /// decrypted message
        /// </summary>
        public static readonly HeaderName Signature = "Platibus-Signature";
        
        /// <summary>
        /// The signature algorithm (e.g. HMACSHA256)
        /// </summary>
        public static readonly HeaderName SignatureAlgorithm = "Platibus-SignatureAlgorithm";
    }
}
