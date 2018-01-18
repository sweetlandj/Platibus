using System;
using System.Threading.Tasks;
using Platibus.Security;
using Xunit;

namespace Platibus.UnitTests.Security
{
    public abstract class MessageEncryptionServiceTests
    {
        protected IMessageEncryptionService EncryptionService;
        protected Message Message;
        protected Message EncryptedMessage;
        protected Message DecryptedMessage;

        protected MessageEncryptionServiceTests(IMessageEncryptionService encryptionService)
        {
            EncryptionService = encryptionService;
        }

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
            return EncryptedMessage = await EncryptionService.Encrypt(Message);
        }

        protected async Task AssertMessageCanBeDecrypted()
        {
            DecryptedMessage = await EncryptionService.Decrypt(EncryptedMessage);
            Assert.Equal(Message, DecryptedMessage, new MessageEqualityComparer());
        }
    }
}
