#if NET452
using System.IdentityModel.Tokens;
#endif
#if NETSTANDARD2_0
using Microsoft.IdentityModel.Tokens;
#endif
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Platibus.IO;

namespace Platibus.Security
{
    /// <inheritdoc />
    /// <summary>
    /// A <see cref="T:Platibus.Security.IMessageEncryptionService" /> that used AES encryption
    /// </summary>
    public class AesMessageEncryptionService : IMessageEncryptionService
    {
        private readonly byte[] _key;

        /// <summary>
        /// Initializes a new <see cref="AesMessageEncryptionService"/> with the specified
        /// <paramref name="key"/>
        /// </summary>
        /// <param name="key">The 128, 192, or 256 bit key used to encrypt and decrypt 
        /// byte streams</param>
        public AesMessageEncryptionService(SymmetricSecurityKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
#if NET452
                _key = key.GetSymmetricKey();
#endif
#if NETSTANDARD2_0
                _key = key.Key;
#endif
        }

        public async Task<Message> Encrypt(Message message)
        {
            byte[] iv;
            using (var csp = new AesCryptoServiceProvider())
            {
                csp.GenerateIV();
                iv = csp.IV;
            }

            var headerCiphertext = await Encrypt(iv, async cryptoStream =>
            {
                using (var messageWriter = new MessageWriter(cryptoStream, Encoding.UTF8, true))
                {
                    await messageWriter.WriteMessageHeaders(message.Headers);
                }
            });

            var contentCiphertext = await Encrypt(iv, async cryptoStream =>
            {
                using (var messageWriter = new MessageWriter(cryptoStream, Encoding.UTF8, true))
                {
                    await messageWriter.WriteMessageContent(message.Content);
                }
            });

            var encryptedHeaders = new EncryptedMessageHeaders
            {
                IV = Convert.ToBase64String(iv),
                Ciphertext = Convert.ToBase64String(headerCiphertext)
            };
            var encryptedContent = Convert.ToBase64String(contentCiphertext);
            return new Message(encryptedHeaders, encryptedContent);
        }

        public async Task<Message> Decrypt(Message encryptedMessage)
        {
            var encryptedHeaders = new EncryptedMessageHeaders(encryptedMessage.Headers);
            var iv = Convert.FromBase64String(encryptedHeaders.IV);
            var headerCiphertext = Convert.FromBase64String(encryptedHeaders.Ciphertext);
            var contentCiphertext = Convert.FromBase64String(encryptedMessage.Content);

            var headers = await Decrypt(headerCiphertext, iv, async cryptoStream =>
            {
                using (var messageReader = new MessageReader(cryptoStream, Encoding.UTF8, true))
                {
                    return await messageReader.ReadMessageHeaders();
                }
            });

            var content = await Decrypt(contentCiphertext, iv, async cryptoStream =>
            {
                using (var messageReader = new MessageReader(cryptoStream, Encoding.UTF8, true))
                {
                    return await messageReader.ReadMessageContent();
                }
            });

            return new Message(headers, content);
        }

        private async Task<byte[]> Encrypt(byte[] iv, Func<CryptoStream, Task> write)
        {
            using (var csp = new AesCryptoServiceProvider())
            {
                var encryptor = csp.CreateEncryptor(_key, iv);
                using (var stream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
                    {
                        await write(cryptoStream);
                    }
                    return stream.ToArray();
                }
            }
        }

        private async Task<TResult> Decrypt<TResult>(byte[] ciphertext, byte[] iv, Func<CryptoStream, Task<TResult>> read)
        {
            using (var csp = new AesCryptoServiceProvider())
            {
                var decryptor = csp.CreateDecryptor(_key, iv);
                using (var stream = new MemoryStream(ciphertext))
                {
                    using (var cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read))
                    {
                        return await read(cryptoStream);
                    }
                }
            }
        }
    }
}
