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
        private bool _disposed;

        public DirectoryInfo BaseDirectory { get; }

        public DirectoryInfo QueueDirectory { get; }

        public SQLiteMessageQueueingService MessageQueueingService { get; }

        public SQLiteSubscriptionTrackingService SubscriptionTrackingService { get; }

        public SQLiteMessageJournal MessageJournal { get; }

        public SQLiteFixture()
        {
            BaseDirectory = GetTempDirectory();
            
            QueueDirectory = CreateSubdirectory(BaseDirectory, "queues");
            MessageQueueingService = new SQLiteMessageQueueingService(QueueDirectory);
            MessageQueueingService.Init();

            var subscriptionDirectory = CreateSubdirectory(BaseDirectory, "subscriptions");
            SubscriptionTrackingService = new SQLiteSubscriptionTrackingService(subscriptionDirectory);
            SubscriptionTrackingService.Init();

            var journalDirectory = CreateSubdirectory(BaseDirectory, "journal");
            MessageJournal = new SQLiteMessageJournal(journalDirectory);
            MessageJournal.Init();
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
            var connection = MessageJournal.ConnectionProvider.GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = "DELETE FROM [PB_MessageJournal]";
                command.ExecuteNonQuery();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            MessageQueueingService.Dispose();
            SubscriptionTrackingService.Dispose();
            MessageJournal.Dispose();
        }
    }
}
