using System;
using System.IO;
using NUnit.Framework;
using Platibus.Filesystem;

namespace Platibus.UnitTests.Filesystem
{
    [SetUpFixture]
    public class FilesystemCollectionFixture
    {
        public static FilesystemCollectionFixture Instance;

        [SetUp]
        public void SetUp()
        {
            Instance = new FilesystemCollectionFixture();
        }

        [TearDown]
        public void TearDown()
        {
            if (Instance != null)
            {
                Instance._messageQueueingService.Dispose();
            }
        }

        private readonly DirectoryInfo _baseDirectory;
        private readonly FilesystemMessageJournalingService _messageJournalingService;
        private readonly FilesystemMessageQueueingService _messageQueueingService;

        public DirectoryInfo BaseDirectory
        {
            get { return _baseDirectory; }
        }

        public FilesystemMessageJournalingService MessageJournalingService
        {
            get { return _messageJournalingService; }
        }

        public FilesystemMessageQueueingService MessageQueueingService
        {
            get { return _messageQueueingService; }
        }

        public FilesystemCollectionFixture()
        {
            _baseDirectory = GetTempDirectory();

            _messageJournalingService = new FilesystemMessageJournalingService(_baseDirectory);
            _messageJournalingService.Init();

            _messageQueueingService = new FilesystemMessageQueueingService(_baseDirectory);
            _messageQueueingService.Init();
        }

        protected DirectoryInfo GetTempDirectory()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "Platibus.UnitTests", DateTime.Now.ToString("yyyyMMddHHmmss"));
            var tempDir = new DirectoryInfo(tempPath);
            if (!tempDir.Exists)
            {
                tempDir.Create();
            }
            return tempDir;
        }
    }
}
