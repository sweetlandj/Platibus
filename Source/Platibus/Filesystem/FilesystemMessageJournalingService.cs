// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
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
using System.IO;
using System.Threading.Tasks;

namespace Platibus.Filesystem
{
    public class FilesystemMessageJournalingService : IMessageJournalingService
    {
        private readonly DirectoryInfo _baseDirectory;
        private readonly DirectoryInfo _receivedDirectory;
        private readonly DirectoryInfo _sentDirectory;
        private readonly DirectoryInfo _publishedDirectory;

        public FilesystemMessageJournalingService(DirectoryInfo baseDirectory = null)
        {
            if (baseDirectory == null)
            {
                var appdomainDirectory = AppDomain.CurrentDomain.BaseDirectory;
                baseDirectory = new DirectoryInfo(Path.Combine(appdomainDirectory, "platibus", "journal"));
            }
            _baseDirectory = baseDirectory;
            _receivedDirectory = new DirectoryInfo(Path.Combine(_baseDirectory.FullName, "received"));
            _sentDirectory = new DirectoryInfo(Path.Combine(_baseDirectory.FullName, "sent"));
            _publishedDirectory = new DirectoryInfo(Path.Combine(_baseDirectory.FullName, "published"));
        }

        public void Init()
        {
            if (!_baseDirectory.Exists)
            {
                _baseDirectory.Create();
            }

            if (!_receivedDirectory.Exists)
            {
                _receivedDirectory.Create();
            }

            if (!_sentDirectory.Exists)
            {
                _sentDirectory.Create();
            }

            if (!_publishedDirectory.Exists)
            {
                _publishedDirectory.Create();
            }
        }

        private static DirectoryInfo GetJournalDirectory(DirectoryInfo baseDirectory, DateTime dateTime)
        {
            var pathSegments = new[]
            {
                baseDirectory.FullName,
                dateTime.Year.ToString("D4"),
                string.Format("{0:D2}{1:D2}", dateTime.Month, dateTime.Day),
                string.Format("{0:D2}{1:D2}", dateTime.Hour, dateTime.Minute)
            };

            var path = Path.Combine(pathSegments);
            return new DirectoryInfo(path);
        }

        public async Task MessageReceived(Message message)
        {
            var receivedDate = message.Headers.Received;
            var directory = GetJournalDirectory(_receivedDirectory, receivedDate);
            if (!directory.Exists)
            {
                directory.Create();
            }
            await MessageFile.Create(directory, message, null);
        }

        public async Task MessageSent(Message message)
        {
            var sentDate = message.Headers.Sent;
            var directory = GetJournalDirectory(_sentDirectory, sentDate);
            if (!directory.Exists)
            {
                directory.Create();
            }
            await MessageFile.Create(directory, message, null);
        }

        public async Task MessagePublished(Message message)
        {
            var publishedDate = message.Headers.Published;
            var topic = message.Headers.Topic;
            var topicDirectory = new DirectoryInfo(Path.Combine(_publishedDirectory.FullName, topic));
            if (!topicDirectory.Exists)
            {
                topicDirectory.Create();
            }
            var directory = GetJournalDirectory(topicDirectory, publishedDate);
            if (!directory.Exists)
            {
                directory.Create();
            }
            await MessageFile.Create(directory, message, null);
        }
    }
}