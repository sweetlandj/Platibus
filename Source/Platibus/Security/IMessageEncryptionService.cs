using System.Threading.Tasks;

namespace Platibus.Security
{
    /// <summary>
    /// Service for encrypting and decrypting data
    /// </summary>
    public interface IMessageEncryptionService
    {
        /// <summary>
        /// Encrypts or otherwise obfuscates the specified <paramref name="message"/>
        /// </summary>
        /// <param name="message">The message to encrypt</param>
        /// <returns>Returns an encrypted version of the message</returns>
        Task<Message> Encrypt(Message message);

        /// <summary>
        /// Decrypts or deciphers an encrypted or obfuscated <paramref name="encryptedMessage"/>
        /// </summary>
        /// <param name="encryptedMessage">The message to decrypt</param>
        /// <returns>Returns the decrypted message</returns>
        Task<Message> Decrypt(Message encryptedMessage);
    }
}
