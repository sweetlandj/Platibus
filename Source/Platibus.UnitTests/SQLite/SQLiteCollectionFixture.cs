using System;
using System.IO;
using NUnit.Framework;
using Platibus.SQLite;

namespace Platibus.UnitTests.SQLite
{
    [SetUpFixture]
    public class SQLiteCollectionFixture
    {
        public static SQLiteCollectionFixture Instance;

        [SetUp]
        public void SetUp()
        {
            Instance = new SQLiteCollectionFixture();
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
        private readonly SQLiteMessageJournalingService _messageJournalingService;
        private readonly SQLiteMessageQueueingService _messageQueueingService;

        public DirectoryInfo BaseDirectory
        {
            get { return _baseDirectory; }
        }

        public SQLiteMessageJournalingService MessageJournalingService
        {
            get { return _messageJournalingService; }
        }

        public SQLiteMessageQueueingService MessageQueueingService
        {
            get { return _messageQueueingService; }
        }

        public SQLiteCollectionFixture()
        {
            _baseDirectory = GetTempDirectory();

            _messageJournalingService = new SQLiteMessageJournalingService(_baseDirectory);
            _messageJournalingService.Init();

            _messageQueueingService = new SQLiteMessageQueueingService(_baseDirectory);
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
