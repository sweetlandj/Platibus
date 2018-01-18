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

    }
}
