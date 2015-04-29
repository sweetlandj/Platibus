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
using System.Collections.Concurrent;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;
using Common.Logging;

namespace Platibus.Filesystem
{
    public class FilesystemMessageQueueingService : IMessageQueueingService
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Filesystem);

        private readonly DirectoryInfo _baseDirectory;
        private readonly ConcurrentDictionary<QueueName, FilesystemMessageQueue> _queues = new ConcurrentDictionary<QueueName, FilesystemMessageQueue>();

        public FilesystemMessageQueueingService(DirectoryInfo baseDirectory = null)
        {
            if (baseDirectory == null)
            {
                var appdomainDirectory = AppDomain.CurrentDomain.BaseDirectory;
                baseDirectory = new DirectoryInfo(Path.Combine(appdomainDirectory, "platibus", "queues"));
            }
            _baseDirectory = baseDirectory;
        }

        public async Task CreateQueue(QueueName queueName, IQueueListener listener,
            QueueOptions options = default(QueueOptions))
        {
            var queueDirectory = new DirectoryInfo(Path.Combine(_baseDirectory.FullName, queueName));
            var queue = new FilesystemMessageQueue(queueDirectory, listener, options);
            if (!_queues.TryAdd(queueName, queue))
            {
                throw new QueueAlreadyExistsException(queueName);
            }

            Log.DebugFormat("Initializing filesystem queue named \"{0}\" in path \"{1}\"...", queueName, queueDirectory);
            await queue.Init().ConfigureAwait(false);
            Log.DebugFormat("Filesystem queue \"{0}\" created successfully", queueName);
        }

        public async Task EnqueueMessage(QueueName queueName, Message message, IPrincipal senderPrincipal)
        {
            FilesystemMessageQueue queue;
            if (!_queues.TryGetValue(queueName, out queue)) throw new QueueNotFoundException(queueName);

            Log.DebugFormat("Enqueueing message ID {0} in filesystem queue \"{1}\"...", message.Headers.MessageId, queueName);
            await queue.Enqueue(message, senderPrincipal).ConfigureAwait(false);
            Log.DebugFormat("Message ID {0} enqueued successfully in filesystem queue \"{1}\"", message.Headers.MessageId, queueName);
        }

        public void Init()
        {
            _baseDirectory.Refresh();
            if (!_baseDirectory.Exists)
            {
                _baseDirectory.Create();
                _baseDirectory.Refresh();
            }
        }
    }
}