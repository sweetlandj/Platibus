namespace Platibus.Security
{
    /// <summary>
    /// Extension methods for working with encrypted messages
    /// </summary>
    public static class EncryptedMessageExceptions
    {
        /// <summary>
        /// Indicates whether the messages is encrypted
        /// </summary>
        /// <param name="message">The message in question</param>
        /// <returns>Returns <c>true</c> if the message is encrypted, <c>false</c> otherwise</returns>
        public static bool IsEncrypted(this Message message)
        {
            return !string.IsNullOrWhiteSpace(message?.Headers[EncryptedHeaderName.Headers]);
        }
    }
}
