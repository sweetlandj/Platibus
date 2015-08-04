// The MIT License (MIT)
// 
// Copyright (c) 2015 Jesse Sweetland
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
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Filesystem
{
    /// <summary>
    /// A message journaling service that stored journaled messages on the
    /// filesystem.
    /// </summary>
    public class FilesystemMessageJournalingService : IMessageJournalingService
    {
        private readonly DirectoryInfo _baseDirectory;
        private readonly DirectoryInfo _receivedDirectory;
        private readonly DirectoryInfo _sentDirectory;
        private readonly DirectoryInfo _publishedDirectory;

        /// <summary>
        /// Creates a new <see cref="FilesystemMessageJournalingService"/> that
        /// will store journaled messages in the specified <paramref name="baseDirectory"/>.
        /// </summary>
        /// <remarks>
        /// Journaled messages will be organized in date-based subdirectories
        /// underneath the <paramref name="baseDirectory"/>.  This is primarily
        /// to avoid issues with large numbers of files in a single directory (which
        /// causes problems for NTFS and Windows Explorer) but also provides a
        /// straightforward means of archival and cleanup.
        /// </remarks>
        /// <param name="baseDirectory">The base directory underneath which journaled
        /// messages will be stored.</param>
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

        /// <summary>
        /// Initializes a newly created <see cref="FilesystemMessageJournalingService"/>
        /// by creating any directories that do not yet exist.
        /// </summary>
        public void Init()
        {
            _baseDirectory.Refresh();
            if (!_baseDirectory.Exists)
            {
                _baseDirectory.Create();
                _baseDirectory.Refresh();
            }

            _receivedDirectory.Refresh();
            if (!_receivedDirectory.Exists)
            {
                _receivedDirectory.Create();
                _receivedDirectory.Refresh();
            }

            _sentDirectory.Refresh();
            if (!_sentDirectory.Exists)
            {
                _sentDirectory.Create();
                _sentDirectory.Refresh();
            }

            _publishedDirectory.Refresh();
            if (!_publishedDirectory.Exists)
            {
                _publishedDirectory.Create();
                _publishedDirectory.Refresh();
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

        public async Task MessageReceived(Message message, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var receivedDate = message.Headers.Received;
            var directory = GetJournalDirectory(_receivedDirectory, receivedDate);
            directory.Refresh();
            if (!directory.Exists)
            {
                directory.Create();
                directory.Refresh();
            }
            await MessageFile.Create(directory, message, null, cancellationToken);
        }

        public async Task MessageSent(Message message, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sentDate = message.Headers.Sent;
            var directory = GetJournalDirectory(_sentDirectory, sentDate);
            directory.Refresh();
            if (!directory.Exists)
            {
                directory.Create();
                directory.Refresh();
            }
            await MessageFile.Create(directory, message, null, cancellationToken);
        }

        public async Task MessagePublished(Message message, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var publishedDate = message.Headers.Published;
            var topic = message.Headers.Topic;
            var topicDirectory = new DirectoryInfo(Path.Combine(_publishedDirectory.FullName, topic));

            topicDirectory.Refresh();
            if (!topicDirectory.Exists)
            {
                topicDirectory.Create();
                topicDirectory.Refresh();
            }

            var directory = GetJournalDirectory(topicDirectory, publishedDate);
            directory.Refresh();
            if (!directory.Exists)
            {
                directory.Create();
                directory.Refresh();
            }
            await MessageFile.Create(directory, message, null, cancellationToken);
        }
    }
}