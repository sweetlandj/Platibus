using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Platibus.Filesystem;

namespace Platibus.UnitTests.Filesystem
{
    internal class FilesystemMessageJournalInspector
    {
        private readonly DirectoryInfo _baseDirectory;

        public FilesystemMessageJournalInspector(DirectoryInfo baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        public async Task<IEnumerable<Message>> EnumerateSentMessages()
        {
            var sentMessagePath = Path.Combine(_baseDirectory.FullName, "sent");
            var sentMessageDirectory = new DirectoryInfo(sentMessagePath);
            var journaledMessageFileTasks = sentMessageDirectory
                .GetFiles("*.pmsg", SearchOption.AllDirectories)
                .Select(file => new MessageFile(file))
                .Select(messageFile => messageFile.ReadMessage());
            return await Task.WhenAll(journaledMessageFileTasks);
        }

        public async Task<IEnumerable<Message>> EnumerateReceivedMessages()
        {
            var receivedMessagePath = Path.Combine(_baseDirectory.FullName, "received");
            var receivedMessageDirectory = new DirectoryInfo(receivedMessagePath);
            var journaledMessageFileTasks = receivedMessageDirectory
                .GetFiles("*.pmsg", SearchOption.AllDirectories)
                .Select(file => new MessageFile(file))
                .Select(messageFile => messageFile.ReadMessage());
            return await Task.WhenAll(journaledMessageFileTasks);
        }

        public async Task<IEnumerable<Message>> EnumeratePublishedMessages()
        {
            var publishedMessagePath = Path.Combine(_baseDirectory.FullName, "published");
            var publishedMessageDirectory = new DirectoryInfo(publishedMessagePath);
            var journaledMessageFileTasks = publishedMessageDirectory
                .GetFiles("*.pmsg", SearchOption.AllDirectories)
                .Select(file => new MessageFile(file))
                .Select(messageFile => messageFile.ReadMessage());
            return await Task.WhenAll(journaledMessageFileTasks);
        }
    }
}