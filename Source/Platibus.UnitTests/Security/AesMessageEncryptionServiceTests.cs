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
using Platibus.Security;
using Xunit;
#if NET452
using System.IdentityModel.Tokens;
#endif
#if NETCOREAPP2_0
using Microsoft.IdentityModel.Tokens;
#endif

namespace Platibus.UnitTests.Security
{
    public class AesMessageEncryptionServiceTests : MessageEncryptionServiceTests
    {
        protected AesMessageEncryptionOptions Options;
        protected IDiagnosticService DiagnosticService = new DiagnosticService();
        protected IList<DiagnosticEvent> EmittedDiagnosticEvents = new List<DiagnosticEvent>();

        public AesMessageEncryptionServiceTests()
        {
            var key = GenerateKey();
            Options = new AesMessageEncryptionOptions(key)
            {
                DiagnosticService = DiagnosticService
            };
            DiagnosticService.AddSink(EmittedDiagnosticEvents.Add);
        }
        
        [Fact]
        public async Task EncryptedMessageCanBeDecryptedWithFallbackKeyAfterKeyMigration()
        {
            GivenMessage();
            await WhenMessageIsEncrypted();
            WhenRotatingKeys();
            await AssertMessageCanBeDecrypted();
        }

        [Fact]
        public async Task EncryptedMessageCannotBeDecryptedWithIncorrectKey()
        {
            GivenMessage();
            await WhenMessageIsEncrypted();
            GivenIncorrectKey();
            await Assert.ThrowsAsync<MessageEncryptionException>(WhenMessageIsDecrypted);
            AssertDiagnosticEvent(DiagnosticEventType.DecryptionError, DiagnosticEventType.SignatureVerificationFailure);
        }

        [Fact]
        public async Task EncryptedMessageWithInvalidSignatureCannotBeDecrypted()
        {
            GivenMessage();
            await WhenMessageIsEncrypted();
            await GivenInvalidSignature();
            await Assert.ThrowsAsync<MessageEncryptionException>(WhenMessageIsDecrypted);
            AssertDiagnosticEvent(DiagnosticEventType.SignatureVerificationFailure);
        }

        [Fact]
        public async Task EncryptedMessageWithMissingSignatureCannotBeDecrypted()
        {
            GivenMessage();
            await WhenMessageIsEncrypted();
            GivenMissingSignature();
            await Assert.ThrowsAsync<MessageEncryptionException>(WhenMessageIsDecrypted);
            AssertDiagnosticEvent(DiagnosticEventType.SignatureVerificationFailure);
        }

        [Fact]
        public async Task EncryptedMessageWithCorruptedSignatureCannotBeDecrypted()
        {
            GivenMessage();
            await WhenMessageIsEncrypted();
            GivenCorruptedSignature();
            await Assert.ThrowsAsync<MessageEncryptionException>(WhenMessageIsDecrypted);
            AssertDiagnosticEvent(DiagnosticEventType.SignatureVerificationFailure);
        }
        
        protected override IMessageEncryptionService GivenEncryptionService()
        {
            return new AesMessageEncryptionService(Options);
        }

        protected void WhenRotatingKeys()
        {
            var replacementKey = GenerateKey();
            var currentKey = Options.Key;
            Options = new AesMessageEncryptionOptions(replacementKey)
            {
                DiagnosticService = DiagnosticService,
                FallbackKeys = new []
                {
                    currentKey
                }
            };
        }

        protected void GivenIncorrectKey()
        {
            var wrongKey = GenerateKey();
            Options = new AesMessageEncryptionOptions(wrongKey)
            {
                DiagnosticService = DiagnosticService
            };
        }

        protected async Task GivenInvalidSignature()
        {
#if NET452
            var key = Options.Key.GetSymmetricKey();
#endif
#if NETCOREAPP2_0
            var key = Options.Key.Key;
#endif
            using (var hmac = new HMACSHA256(key))
            {
                var originalMessageHeaders = Message.Headers;
                var modifiedHeaders = new MessageHeaders(originalMessageHeaders)
                {
                    Origination = new Uri("urn:localhost/platibus2")
                };
                using (var cleartextStream = new MemoryStream())
                {
                    using (var messageWriter = new MessageWriter(cleartextStream, Encoding.UTF8, true))
                    {
                        await messageWriter.WriteMessageHeaders(modifiedHeaders);
                    }
                    var invalidSignature = hmac.ComputeHash(cleartextStream);
                    var encryptedHeaders = new EncryptedMessageHeaders(EncryptedMessage.Headers)
                    {
                        Signature = Convert.ToBase64String(invalidSignature)
                    };

                    EncryptedMessage = new Message(encryptedHeaders, EncryptedMessage.Content);
                }
            }
        }

        protected void GivenMissingSignature()
        {
            var encryptedHeaders = new EncryptedMessageHeaders(EncryptedMessage.Headers)
            {
                Signature = null
            };

            EncryptedMessage = new Message(encryptedHeaders, EncryptedMessage.Content);
        }

        protected void GivenCorruptedSignature()
        {
            var encryptedHeaders = new EncryptedMessageHeaders(EncryptedMessage.Headers)
            {
                Signature = Convert.ToBase64String(Encoding.UTF8.GetBytes("Bad signature"))
            };

            EncryptedMessage = new Message(encryptedHeaders, EncryptedMessage.Content);
        }

        protected void AssertDiagnosticEvent(params DiagnosticEventType[] types)
        {
            Assert.Contains(EmittedDiagnosticEvents, e => types.Contains(e.Type));
        }

        protected SymmetricSecurityKey GenerateKey() => KeyGenerator.GenerateAesKey();
    }
}
