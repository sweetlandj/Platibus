using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Platibus.Filesystem;
using Platibus.Security;
using Platibus.UnitTests.Security;
using Xunit;
#if NET452
using Platibus.Config;
#endif
#if NETCOREAPP2_0
using Microsoft.Extensions.Configuration;
#endif

namespace Platibus.UnitTests.Filesystem
{
    [Trait("Category", "UnitTests")]
    [Collection(FilesystemCollection.Name)]
    public class FilesystemServicesProviderMessageQueueingServiceTests
    {
        protected readonly DirectoryInfo Path;

        protected Message Message;
        protected QueueName Queue = Guid.NewGuid().ToString();
        protected IQueueListener QueueListener = new Mock<IQueueListener>().Object;
#if NET452
        protected QueueingElement Configuration = new QueueingElement();
#endif
#if NETCOREAPP2_0
        protected IConfiguration Configuration;
#endif

        public FilesystemServicesProviderMessageQueueingServiceTests(FilesystemFixture fixture)
        {
#if NETCOREAPP2_0
            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();

#endif
            Path = fixture.BaseDirectory;
            Message = new Message(new MessageHeaders
            {
                MessageId = MessageId.Generate()
            }, "FilesystemServicesProviderMessageQueueingTests");
        }

        [Fact]
        public async Task PathHasDefaultValue()
        {
            var defaultPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "platibus", "queues");
            GivenExplicitPath(null);
            await WhenMessageEnqueued();
            await AssertMessageFileCreated(defaultPath);
        }

        [Fact]
        public async Task PathCanBeOverridden()
        {
            var pathOverride = System.IO.Path.Combine(Path.FullName, "override");
            GivenExplicitPath(pathOverride);
            await WhenMessageEnqueued();
            await AssertMessageFileCreated(pathOverride);
        }

        [Fact]
        public async Task MessagesCanBeEncrypted()
        {
            GivenExplicitPath(Path.FullName);
            GivenEncryption();
            await WhenMessageEnqueued();
            await AssertEncryptedMessageFileCreated(Path.FullName);
        }
        
        protected void GivenExplicitPath(string path)
        {
            ConfigureAttribute("path", path);
        }

        protected void GivenEncryption()
        {
#if NET452
            Configuration.Encryption = new EncryptionElement
            {
                Enabled = true,
                Provider = "AES",
                Key = HexEncoding.GetString(KeyGenerator.GenerateAesKey().GetSymmetricKey())
            };
#endif
#if NETCOREAPP2_0
            var section = Configuration.GetSection("encryption");
            section["enabled"] = "true";
            section["provider"] = "aes";
            section["key"] = HexEncoding.GetString(KeyGenerator.GenerateAesKey().Key);
#endif
        }
        
        protected async Task WhenMessageEnqueued()
        {
            var messageQueueingService = await new FilesystemServicesProvider().CreateMessageQueueingService(Configuration);
            await messageQueueingService.CreateQueue(Queue, QueueListener);
            await messageQueueingService.EnqueueMessage(Queue, Message, null);
        }

        protected async Task AssertMessageFileCreated(string path)
        {
            var message = await ReadMessage(path);
            Assert.NotNull(message);
        }

        protected async Task AssertEncryptedMessageFileCreated(string path)
        {
            var message = await ReadMessage(path);
            Assert.NotNull(message);
            Assert.True(message.IsEncrypted());
        }

        private async Task<Message> ReadMessage(string path)
        {
            var filenamePattern = Message.Headers.MessageId + "*.pmsg";
            var queuePath = System.IO.Path.Combine(path, Queue);
            var queueDirectory = new DirectoryInfo(queuePath);
            var messageFile = queueDirectory.GetFiles(filenamePattern).FirstOrDefault();
            return messageFile == null
                ? null 
                : await new MessageFile(messageFile).ReadMessage();
        }

        protected void ConfigureAttribute(string name, string value)
        {
#if NET452
            Configuration.SetAttribute(name, value);
#endif
#if NETCOREAPP2_0
            Configuration[name] = value;
#endif
        }
    }
}