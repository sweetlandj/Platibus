using Platibus.Filesystem;
using System;
using System.IO;
using Platibus.Diagnostics;

namespace Platibus.UnitTests.Filesystem
{
    public class FilesystemFixture : IDisposable
    {
        private bool _disposed;

        public IDiagnosticService DiagnosticService { get; } = new DiagnosticService();

        public DirectoryInfo BaseDirectory { get; }

        public FilesystemMessageQueueingService MessageQueueingService { get; }

        public FilesystemSubscriptionTrackingService SubscriptionTrackingService { get; }

        public FilesystemFixture()
        {
            BaseDirectory = GetTempDirectory();

            var fsQueueingOptions = new FilesystemMessageQueueingOptions
            {
                DiagnosticService = DiagnosticService,
                BaseDirectory = BaseDirectory
            };
            
            MessageQueueingService = new FilesystemMessageQueueingService(fsQueueingOptions);
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
