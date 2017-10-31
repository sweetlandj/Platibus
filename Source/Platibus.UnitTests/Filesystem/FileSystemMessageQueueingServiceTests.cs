using Platibus.Diagnostics;
using Platibus.Filesystem;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading.Tasks;
using Xunit;

namespace Platibus.UnitTests.Filesystem
{
    [Trait("Category", "UnitTests")]
    [Collection(FilesystemCollection.Name)]
    public class FileSystemMessageQueueingServiceTests : MessageQueueingServiceTests<FilesystemMessageQueueingService>
    {
        private readonly DirectoryInfo _baseDirectory;
        
        public FileSystemMessageQueueingServiceTests(FilesystemFixture fixture)
            : base(fixture.MessageQueueingService)
        {
            _baseDirectory = fixture.BaseDirectory;
        }

        [Fact]
        public async Task MalformedMessageFilesShouldPreventQueueCreation()
        {
            var queue = GivenUniqueQueueName();
            var path = GivenExistingMalformedMessage(queue);

            var sink = new VerificationSink();
            DiagnosticService.DefaultInstance.AddSink(sink);
            try
            {
                var listener = new QueueListenerStub();
                await MessageQueueingService.CreateQueue(queue, listener);
            }
            finally
            {
                DiagnosticService.DefaultInstance.RemoveSink(sink);
            }

            sink.VerifyEmitted<FilesystemEvent>(
                FilesystemEventType.MessageFileFormatError, 
                e => e.Path == path && e.Exception != null);
        }

        protected override async Task GivenExistingQueuedMessage(QueueName queueName, Message message, IPrincipal principal)
        {
            var queueDirectory = QueueDirectory(queueName);
            await MessageFile.Create(queueDirectory, message);
        }

        protected string GivenExistingMalformedMessage(QueueName queueName)
        {
            var rng = new RNGCryptoServiceProvider();
            var randomBytes = new byte[1000];
            rng.GetBytes(randomBytes);

            var messageId = MessageId.Generate();
            var queueDirectory = QueueDirectory(queueName);
            var filename = messageId + ".pmsg";
            var path = Path.Combine(queueDirectory.FullName, filename);
            File.WriteAllBytes(path, randomBytes);
            return path;
        }

        protected override Task<bool> MessageQueued(QueueName queueName, Message message)
        {
            var queueDirectory = QueueDirectory(queueName);
            var queuedFiles = queueDirectory.EnumerateFiles();
            var messageId = message.Headers.MessageId.ToString();
            var messageInQueue = queuedFiles.Any(f => f.Name.StartsWith(messageId));
            return Task.FromResult(messageInQueue);
        }

        protected override Task<bool> MessageDead(QueueName queueName, Message message)
        {
            var queueDirectory = QueueDirectory(queueName);
            var deadLetterDirectory = new DirectoryInfo(Path.Combine(queueDirectory.FullName, "dead"));
            var deadLetters = deadLetterDirectory.EnumerateFiles();
            var messageId = message.Headers.MessageId.ToString();
            var isDead = deadLetters.Any(f => f.Name.StartsWith(messageId));
            return Task.FromResult(isDead);
        }

        private DirectoryInfo QueueDirectory(QueueName queueName)
        {
            var queuePath = Path.Combine(_baseDirectory.FullName, queueName);
            var queueDirectory = new DirectoryInfo(queuePath);
            queueDirectory.Refresh();
            if (!queueDirectory.Exists)
            {
                queueDirectory.Create();
            }
            return queueDirectory;
        }
    }
}
