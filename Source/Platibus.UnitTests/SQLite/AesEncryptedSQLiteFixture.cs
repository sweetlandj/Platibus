using System;
using System.Data;
using System.IO;
using Platibus.Security;
using Platibus.SQLite;
using Platibus.UnitTests.Security;

namespace Platibus.UnitTests.SQLite
{
    public class AesEncryptedSQLiteFixture : IDisposable
    {
        private bool _disposed;

        public DirectoryInfo BaseDirectory { get; }

        public DirectoryInfo QueueDirectory { get; }

        public AesMessageEncryptionService MessageEncryptionService { get; }

        public SQLiteMessageQueueingService MessageQueueingService { get; }

        public AesEncryptedSQLiteFixture()
        {
            BaseDirectory = GetTempDirectory();

            var aesOptions = new AesMessageEncryptionOptions(KeyGenerator.GenerateAesKey());
            MessageEncryptionService = new AesMessageEncryptionService(aesOptions);
            
            QueueDirectory = CreateSubdirectory(BaseDirectory, "queues");
            var queueingOptions = new SQLiteMessageQueueingOptions
            {
                BaseDirectory = QueueDirectory,
                MessageEncryptionService = MessageEncryptionService
            };
            MessageQueueingService = new SQLiteMessageQueueingService(queueingOptions);
            MessageQueueingService.Init();
        }

        private static DirectoryInfo CreateSubdirectory(DirectoryInfo baseDirectory, string name)
        {
            var path = Path.Combine(baseDirectory.FullName, name);
            var subdirectory = new DirectoryInfo(path);
            subdirectory.Refresh();
            if (!subdirectory.Exists)
            {
                subdirectory.Create();
            }
            return subdirectory;
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
            MessageQueueingService.Dispose();
        }
    }
}