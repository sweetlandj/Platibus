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
