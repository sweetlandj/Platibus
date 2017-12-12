using System;
using System.IO;
using Platibus.Filesystem;

namespace Platibus.UnitTests.Filesystem
{
    public class FilesystemFixture : IDisposable
    {
        private bool _disposed;

        public DirectoryInfo BaseDirectory { get; }

        public FilesystemMessageQueueingService MessageQueueingService { get; }

        public FilesystemSubscriptionTrackingService SubscriptionTrackingService { get; }

        public FilesystemFixture()
        {
            BaseDirectory = GetTempDirectory();
            
            MessageQueueingService = new FilesystemMessageQueueingService(BaseDirectory);
            MessageQueueingService.Init();

            SubscriptionTrackingService = new FilesystemSubscriptionTrackingService(BaseDirectory);
            SubscriptionTrackingService.Init();
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
        
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                MessageQueueingService.Dispose();
                SubscriptionTrackingService.Dispose();
            }
        }
    }
}
