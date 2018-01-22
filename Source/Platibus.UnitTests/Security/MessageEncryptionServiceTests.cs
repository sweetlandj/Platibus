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
using System.Threading.Tasks;
using Platibus.Security;
using Xunit;

namespace Platibus.UnitTests.Security
{
    public abstract class MessageEncryptionServiceTests
    {
        protected Message Message;
        protected Message EncryptedMessage;
        protected Message DecryptedMessage;

        [Fact]
        public async Task EncryptedMessageCanBeDecrypted()
        {
            GivenMessage();
            await WhenMessageIsEncrypted();
            await AssertMessageCanBeDecrypted();
        }
        
        protected Message GivenMessage()
        {
            var messageHeaders = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                Sent = DateTime.UtcNow,
                Origination = new Uri("urn:localhost/platibus0"),
                Destination = new Uri("urn:localhost/platibus1"),
                MessageName = "TestMessage",
                ContentType = "text/plain"
            };
            return Message = new Message(messageHeaders, "Hello, world!");
        }

        protected async Task<Message> WhenMessageIsEncrypted()
        {
            var encryptionService = GivenEncryptionService();
            return EncryptedMessage = await encryptionService.Encrypt(Message);
        }

        protected async Task<Message> WhenMessageIsDecrypted()
        {
            var encryptionService = GivenEncryptionService();
            return DecryptedMessage = await encryptionService.Decrypt(EncryptedMessage);
        }

        protected async Task AssertMessageCanBeDecrypted()
        {
            if (DecryptedMessage == null)
            {
                await WhenMessageIsDecrypted();
            }
            Assert.Equal(Message, DecryptedMessage, new MessageEqualityComparer());
        }

        protected abstract IMessageEncryptionService GivenEncryptionService();
    }
}
