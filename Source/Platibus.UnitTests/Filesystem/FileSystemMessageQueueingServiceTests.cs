using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Platibus.Filesystem;

namespace Platibus.UnitTests.Filesystem
{
    public class FileSystemMessageQueueingServiceTests : MessageQueueingServiceTests<FilesystemMessageQueueingService>
    {
        private readonly DirectoryInfo _baseDirectory;

        public FileSystemMessageQueueingServiceTests() 
            : this(FilesystemCollectionFixture.Instance)
        {
        }

        public FileSystemMessageQueueingServiceTests(FilesystemCollectionFixture fixture)
            : base(fixture.MessageQueueingService)
        {
            _baseDirectory = fixture.BaseDirectory;
        }

        protected override async Task GivenExistingQueuedMessage(QueueName queueName, Message message, IPrincipal principal)
        {
            var queueDirectory = QueueDirectory(queueName);
            await MessageFile.Create(queueDirectory, message);
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
