using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using NUnit.Framework;
using Platibus.Filesystem;

namespace Platibus.UnitTests
{
    internal class MessageFileTests
    {
        protected DirectoryInfo GetTempDirectory()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "Platibus.UnitTests",
                DateTime.Now.ToString("yyyyMMddHHmmss"));
            var tempDir = new DirectoryInfo(tempPath);
            if (!tempDir.Exists)
            {
                tempDir.Create();
            }
            return tempDir;
        }

        [Test]
        public async Task Given_No_Principal_When_Reading_Principal_Should_Be_Null()
        {
            var tempDir = GetTempDirectory();
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var queuePath = Path.Combine(tempDir.FullName, queueName);
            var queueDir = new DirectoryInfo(queuePath);
            if (!queueDir.Exists)
            {
                queueDir.Create();
            }

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            var file = (await MessageFile.Create(queueDir, message, null)).File;
            var messageFile = new MessageFile(file);
            var readSenderPrincipal = await messageFile.ReadSenderPrincipal();
            var readMessage = await messageFile.ReadMessage();

            Assert.That(readSenderPrincipal, Is.Null);
            Assert.That(readMessage, Is.EqualTo(message).Using(new MessageEqualityComparer()));
        }

        [Test]
        public async Task Given_ClaimsPrincipal_When_Reading_Principal_Should_Be_Read()
        {
            var tempDir = GetTempDirectory();
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var queuePath = Path.Combine(tempDir.FullName, queueName);
            var queueDir = new DirectoryInfo(queuePath);
            if (!queueDir.Exists)
            {
                queueDir.Create();
            }

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            var senderPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("username", "testuser"),
                new Claim("role", "testrole")
            }));

            var file = (await MessageFile.Create(queueDir, message, senderPrincipal)).File;
            var messageFile = new MessageFile(file);
            var readSenderPrincipal = await messageFile.ReadSenderPrincipal();
            var readMessage = await messageFile.ReadMessage();

            Assert.That(readSenderPrincipal, Is.EqualTo(senderPrincipal).Using(new ClaimsPrincipalEqualityComparer()));
            Assert.That(readMessage, Is.EqualTo(message).Using(new MessageEqualityComparer()));
        }
    }
}