using System;
using System.IO;
using System.Threading.Tasks;
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
                Instance._messageQueueingService.TryDispose();
                Instance._subscriptionTrackingService.TryDispose();
            }
        }

        private readonly DirectoryInfo _baseDirectory;
        private readonly FilesystemMessageJournalingService _messageJournalingService;
        private readonly FilesystemMessageQueueingService _messageQueueingService;
        private readonly FilesystemSubscriptionTrackingService _subscriptionTrackingService;

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

        public FilesystemSubscriptionTrackingService SubscriptionTrackingService
        {
            get { return _subscriptionTrackingService; }
        }

        public FilesystemCollectionFixture()
        {
            _baseDirectory = GetTempDirectory();

            _messageJournalingService = new FilesystemMessageJournalingService(_baseDirectory);
            _messageJournalingService.Init();

            _messageQueueingService = new FilesystemMessageQueueingService(_baseDirectory);
            _messageQueueingService.Init();

            _subscriptionTrackingService = new FilesystemSubscriptionTrackingService(_baseDirectory);
            _subscriptionTrackingService.Init();
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
