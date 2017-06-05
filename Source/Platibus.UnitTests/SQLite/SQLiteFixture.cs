// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Data;
using System.IO;
using Platibus.SQLite;

namespace Platibus.UnitTests.SQLite
{
    public class SQLiteFixture : IDisposable
    {
        private readonly DirectoryInfo _baseDirectory;
        private readonly DirectoryInfo _queueDirectory;

        private readonly SQLiteMessageQueueingService _messageQueueingService;
        private readonly SQLiteSubscriptionTrackingService _subscriptionTrackingService;
        private readonly SQLiteMessageJournal _messageJournal;

        private bool _disposed;

        public DirectoryInfo BaseDirectory
        {
            get { return _baseDirectory; }
        }

        public DirectoryInfo QueueDirectory
        {
            get { return _queueDirectory; }
        }

        public SQLiteMessageQueueingService MessageQueueingService
        {
            get { return _messageQueueingService; }
        }

        public SQLiteSubscriptionTrackingService SubscriptionTrackingService
        {
            get { return _subscriptionTrackingService; }
        }

        public SQLiteMessageJournal MessageJournal
        {
            get { return _messageJournal; }
        }

        public SQLiteFixture()
        {
            _baseDirectory = GetTempDirectory();
            
            _queueDirectory = CreateSubdirectory(_baseDirectory, "queues");
            _messageQueueingService = new SQLiteMessageQueueingService(_queueDirectory);
            _messageQueueingService.Init();

            var subscriptionDirectory = CreateSubdirectory(_baseDirectory, "subscriptions");
            _subscriptionTrackingService = new SQLiteSubscriptionTrackingService(subscriptionDirectory);
            _subscriptionTrackingService.Init();

            var journalDirectory = CreateSubdirectory(_baseDirectory, "journal");
            _messageJournal = new SQLiteMessageJournal(journalDirectory);
            _messageJournal.Init();
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

        public void DeleteJournaledMessages()
        {
            // Do not dispose connection.  Singleton connection provider used.
            var connection = _messageJournal.ConnectionProvider.GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = "DELETE FROM [PB_MessageJournal]";
                command.ExecuteNonQuery();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_subscriptionTrackingService")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_messageQueueingService")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_messageJournal")]
        protected virtual void Dispose(bool disposing)
        {
            _messageQueueingService.TryDispose();
            _subscriptionTrackingService.TryDispose();
            _messageJournal.TryDispose();
        }
    }
}
