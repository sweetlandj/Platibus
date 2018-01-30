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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Platibus.Diagnostics;
using Platibus.IO;

namespace Platibus.Security
{
    /// <inheritdoc />
    /// <summary>
    /// A <see cref="T:Platibus.Security.IMessageEncryptionService" /> that used AES encryption
    /// </summary>
    public class AesMessageEncryptionService : IMessageEncryptionService
    {
        private readonly IDiagnosticService _diagnosticService;
        private readonly byte[] _encryptionKey;
        private readonly IList<byte[]> _decryptionKeys;

        /// <summary>
        /// Initializes a new <see cref="AesMessageEncryptionService"/> with the specified
        /// <paramref name="options"/>
        /// </summary>
        /// <param name="options">Options that influence the behavior of the AES message
        /// encryption service</param>
        public AesMessageEncryptionService(AesMessageEncryptionOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _diagnosticService = options.DiagnosticService;
#if NET452
            _encryptionKey = options.Key.GetSymmetricKey();
            _decryptionKeys = new[] {_encryptionKey}.Union(
                options.FallbackKeys?
                    .Where(k => k != null)
                    .Select(k => k.GetSymmetricKey())
                    .ToList()
                ?? Enumerable.Empty<byte[]>())
                .ToList();
#endif
#if NETSTANDARD2_0
            _encryptionKey = options.Key.Key;
            _decryptionKeys = new[] {_encryptionKey}.Union(
                    options.FallbackKeys?
                        .Where(k => k != null)
                        .Select(k => k.Key)
                        .ToList()
                    ?? Enumerable.Empty<byte[]>())
                .ToList();
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

            var headerCleartext = await MarshalHeaders(message.Headers);
            var contentCleartext = await MarshalContent(message.Content);
            var headerCiphertext = await Encrypt(iv, headerCleartext);
            var contentCiphertext = await Encrypt(iv, contentCleartext);
            var headerSignature = Sign(headerCleartext);

            var encryptedHeaders = new EncryptedMessageHeaders
            {
                // Message ID must be available in cleartext
                MessageId = message.Headers.MessageId,
                IV = Convert.ToBase64String(iv),
                Headers = Convert.ToBase64String(headerCiphertext),
                Signature = Convert.ToBase64String(headerSignature),
                SignatureAlgorithm = "HMACSHA256"
            };
            var encryptedContent = Convert.ToBase64String(contentCiphertext);
            return new Message(encryptedHeaders, encryptedContent);
        }

        public async Task<Message> Decrypt(Message encryptedMessage)
        {
            var encryptedHeaders = new EncryptedMessageHeaders(encryptedMessage.Headers);
            var iv = Convert.FromBase64String(encryptedHeaders.IV);
            var headerCiphertext = Convert.FromBase64String(encryptedHeaders.Headers);
            var contentCiphertext = Convert.FromBase64String(encryptedMessage.Content);
            
            var signature = DecodeSignature(encryptedMessage, encryptedHeaders);
            var keyNumber = 0;
            var keyCount = _decryptionKeys.Count;
            var innerExceptions = new List<Exception>();
            foreach (var key in _decryptionKeys)
            {
                keyNumber++;
                byte[] headerCleartext;
                byte[] contentCleartext;
                try
                {
                    headerCleartext = await Decrypt(headerCiphertext, key, iv);
                }
                catch (Exception ex)
                {
                    innerExceptions.Add(ex);
                    _diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.DecryptionError)
                    {
                        Detail = $"Error decrypting message headers using key {keyNumber} of {keyCount}",
                        Message = encryptedMessage,
                        Exception = ex
                    }.Build());
                    continue;
                }

                var signatureVerified = false;
                try
                {
                    signatureVerified = Verify(key, headerCleartext, signature);
                    if (!signatureVerified)
                    {
                        _diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.SignatureVerificationFailure)
                        {
                            Detail = $"Signature verification failed using key {keyNumber} of {keyCount}",
                            Message = encryptedMessage
                        }.Build());
                    }
                }
                catch (Exception ex)
                {
                    innerExceptions.Add(ex);
                    _diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.SignatureVerificationFailure)
                    {
                        Detail = $"Unexpected error verifying message signature using key {keyNumber} of {keyCount}",
                        Message = encryptedMessage,
                        Exception = ex
                    }.Build());
                }

                if (!signatureVerified) continue;

                try
                {
                    contentCleartext = await Decrypt(contentCiphertext, key, iv);
                }
                catch (Exception ex)
                {
                    innerExceptions.Add(ex);
                    _diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.DecryptionError)
                    {
                        Detail = $"Error decrypting message content using key {keyNumber} of {keyCount}",
                        Message = encryptedMessage,
                        Exception = ex
                    }.Build());
                    continue;
                }

                var headers = await UnmarshalHeaders(headerCleartext);
                var content = await UnmarshalContent(contentCleartext);
                return new Message(headers, content);
            }

            throw new MessageEncryptionException($"Unable to decrypt and verify message using any of {keyCount} available decryption key(s)", innerExceptions);
        }

        private byte[] DecodeSignature(Message encryptedMessage, EncryptedMessageHeaders encryptedHeaders)
        {
            if (string.IsNullOrWhiteSpace(encryptedHeaders.Signature))
            {
                _diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.SignatureVerificationFailure)
                {
                    Detail = "Missing signature",
                    Message = encryptedMessage
                }.Build());

                throw new MessageEncryptionException("Missing signature");
            }

            byte[] signature;
            try
            {
                signature = Convert.FromBase64String(encryptedHeaders.Signature);
            }
            catch (Exception ex)
            {
                _diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.SignatureVerificationFailure)
                {
                    Detail = "Error decoding signature",
                    Message = encryptedMessage,
                    Exception = ex
                }.Build());

                throw new MessageEncryptionException("Error decoding signature", ex);
            }

            return signature;
        }

        private static async Task<byte[]> MarshalHeaders(IMessageHeaders headers)
        {
            using (var stream = new MemoryStream())
            {
                using (var messageWriter = new MessageWriter(stream, Encoding.UTF8, true))
                {
                    await messageWriter.WriteMessageHeaders(headers);
                }
                return stream.ToArray();
            }
        }

        private static async Task<IMessageHeaders> UnmarshalHeaders(byte[] marshaledHeaders)
        {
            using (var stream = new MemoryStream(marshaledHeaders))
            {
                using (var messageReader = new MessageReader(stream, Encoding.UTF8, true))
                {
                    return await messageReader.ReadMessageHeaders();
                }
            }
        }

        private static async Task<byte[]> MarshalContent(string content)
        {
            using (var stream = new MemoryStream())
            {
                using (var messageWriter = new MessageWriter(stream, Encoding.UTF8, true))
                {
                    await messageWriter.WriteMessageContent(content);
                }
                return stream.ToArray();
            }
        }

        private static async Task<string> UnmarshalContent(byte[] marshaledContent)
        {
            using (var stream = new MemoryStream(marshaledContent))
            {
                using (var messageReader = new MessageReader(stream, Encoding.UTF8, true))
                {
                    return await messageReader.ReadMessageContent();
                }
            }
        }
        
        private async Task<byte[]> Encrypt(byte[] iv, byte[] cleartext)
        {
            using (var csp = new AesCryptoServiceProvider())
            {
                var encryptor = csp.CreateEncryptor(_encryptionKey, iv);
                using (var ciphertextStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(ciphertextStream, encryptor, CryptoStreamMode.Write))
                    {
                        await cryptoStream.WriteAsync(cleartext, 0, cleartext.Length);
                    }
                    return ciphertextStream.ToArray();
                }
            }
        }

        private byte[] Sign(byte[] message)
        {
            using (var alg = new HMACSHA256(_encryptionKey))
            {
                return alg.ComputeHash(message);
            }
        }

        private static async Task<byte[]> Decrypt(byte[] ciphertext, byte[] key, byte[] iv)
        {
            using (var csp = new AesCryptoServiceProvider())
            {
                var decryptor = csp.CreateDecryptor(key, iv);
                using (var ciphertextStream = new MemoryStream(ciphertext))
                using (var cryptoStream = new CryptoStream(ciphertextStream, decryptor, CryptoStreamMode.Read))
                using (var cleartextStream = new MemoryStream())
                {
                    await cryptoStream.CopyToAsync(cleartextStream);
                    return cleartextStream.ToArray();
                }
            }
        }

        private static bool Verify(byte[] key, byte[] message, byte[] signature)
        {
            using (var alg = new HMACSHA256(key))
            {
                var hash = alg.ComputeHash(message);
                if (hash.Length != signature.Length) return false;

                for (var i = 0; i < hash.Length; i++)
                {
                    if (hash[i] != signature[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
