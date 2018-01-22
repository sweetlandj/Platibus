using System;
using System.IO;
using Platibus.Diagnostics;
using Platibus.Filesystem;
using Platibus.Security;
using Platibus.UnitTests.Security;

namespace Platibus.UnitTests.Filesystem
{
    public class AesEncryptedFilesystemFixture : IDisposable
    {
        private bool _disposed;

        public DirectoryInfo BaseDirectory { get; }

        public AesMessageEncryptionService MessageEncryptionService { get; }

        public FilesystemMessageQueueingService MessageQueueingService { get; }

        public AesEncryptedFilesystemFixture()
        {
            BaseDirectory = GetTempDirectory();

            var aesOptions = new AesMessageEncryptionOptions(KeyGenerator.GenerateAesKey());
            MessageEncryptionService = new AesMessageEncryptionService(aesOptions);

            var fsQueueingOptions = new FilesystemMessageQueueingOptions
            {
                BaseDirectory = BaseDirectory,
                MessageEncryptionService = MessageEncryptionService
            };
            
            MessageQueueingService = new FilesystemMessageQueueingService(fsQueueingOptions);
            MessageQueueingService.Init();
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
            }
        }
    }
}