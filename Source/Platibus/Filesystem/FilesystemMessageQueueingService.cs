// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
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
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Platibus.Security;

namespace Platibus.Filesystem
{
    /// <summary>
    /// A see <see cref="IMessageQueueingService"/> that queues messages as files on disk
    /// </summary>
    public class FilesystemMessageQueueingService : IMessageQueueingService, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Filesystem);

        private readonly DirectoryInfo _baseDirectory;
        private readonly IMessageSecurityTokenService _messageSecurityTokenService;
        private readonly ConcurrentDictionary<QueueName, FilesystemMessageQueue> _queues =
            new ConcurrentDictionary<QueueName, FilesystemMessageQueue>();

        private bool _disposed;

        /// <summary>
        /// Initializes a new <see cref="FilesystemMessageQueueingService"/> that will create
        /// directories and files relative to the specified <paramref name="baseDirectory"/>
        /// </summary>
        /// <param name="baseDirectory">(Optional) The directory in which queued message files
        /// will be stored</param>
        /// <param name="messageSecurityTokenService">(Optional) The message security token
        /// service to use to issue and validate security tokens for persisted messages.</param>
        /// <remarks>
        /// <para>If a base directory is not specified then the base directory will default to a
        /// directory named <c>platibus\queues</c> beneath the current app domain base 
        /// directory.  If the base directory does not exist it will be created in the
        /// <see cref="Init"/> method.</para>
        /// <para>If a <paramref name="messageSecurityTokenService"/> is not specified then a
        /// default implementation based on unsigned JWTs will be used.</para>
        /// </remarks>
        public FilesystemMessageQueueingService(DirectoryInfo baseDirectory = null, IMessageSecurityTokenService messageSecurityTokenService = null)
        {
            if (baseDirectory == null)
            {
                var appdomainDirectory = AppDomain.CurrentDomain.BaseDirectory;
                baseDirectory = new DirectoryInfo(Path.Combine(appdomainDirectory, "platibus", "queues"));
            }
            _baseDirectory = baseDirectory;
            _messageSecurityTokenService = messageSecurityTokenService ?? new JwtMessageSecurityTokenService();
        }

        /// <summary>
        /// Establishes a named queue
        /// </summary>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="listener">An object that will receive messages off of the queue for processing</param>
        /// <param name="options">(Optional) Options that govern how the queue behaves</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used
        /// by the caller to cancel queue creation if necessary</param>
        /// <returns>Returns a task that will complete when the queue has been created</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queueName"/> or
        /// <paramref name="listener"/> is <c>null</c></exception>
        /// <exception cref="QueueAlreadyExistsException">Thrown if a queue with the specified
        /// name already exists</exception>
        public async Task CreateQueue(QueueName queueName, IQueueListener listener, QueueOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            var queueDirectory = new DirectoryInfo(Path.Combine(_baseDirectory.FullName, queueName));
            var queue = new FilesystemMessageQueue(queueDirectory, listener, _messageSecurityTokenService, options);
            if (!_queues.TryAdd(queueName, queue))
            {
                throw new QueueAlreadyExistsException(queueName);
            }

            cancellationToken.ThrowIfCancellationRequested();

            Log.DebugFormat("Initializing filesystem queue named \"{0}\" in path \"{1}\"...", queueName, queueDirectory);
            await queue.Init(cancellationToken);
            Log.DebugFormat("Filesystem queue \"{0}\" created successfully", queueName);
        }

        /// <summary>
        /// Enqueues a message on a queue
        /// </summary>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="message">The message to enqueue</param>
        /// <param name="senderPrincipal">(Optional) The sender principal, if applicable</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be
        /// used be the caller to cancel the enqueue operation if necessary</param>
        /// <returns>Returns a task that will complete when the message has been enqueued</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queueName"/>
        /// or <paramref name="message"/> is <c>null</c></exception>
        public async Task EnqueueMessage(QueueName queueName, Message message, IPrincipal senderPrincipal, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            FilesystemMessageQueue queue;
            if (!_queues.TryGetValue(queueName, out queue)) throw new QueueNotFoundException(queueName);

            Log.DebugFormat("Enqueueing message ID {0} in filesystem queue \"{1}\"...", message.Headers.MessageId, queueName);
            await queue.Enqueue(message, senderPrincipal, cancellationToken);
            Log.DebugFormat("Message ID {0} enqueued successfully in filesystem queue \"{1}\"", message.Headers.MessageId, queueName);
        }

        /// <summary>
        /// Initializes the fileystem queueing service
        /// </summary>
        /// <remarks>
        /// Creates directories if they do not exist
        /// </remarks>
        public void Init()
        {
            _baseDirectory.Refresh();
            if (!_baseDirectory.Exists)
            {
                _baseDirectory.Create();
                _baseDirectory.Refresh();
            }
        }

        /// <summary>
        /// Finalizer that ensures that resources are released
        /// </summary>
        ~FilesystemMessageQueueingService()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called by <see cref="Dispose()"/> or the finalizer to ensure that resources are released
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the 
        /// <see cref="Dispose()"/> method (<c>true</c>) or the finalizer (<c>false</c>)</param>
        /// <remarks>
        /// This method will not be called more than once
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                var queueNames = _queues.Keys;
                foreach (var queueName in queueNames)
                {
                    FilesystemMessageQueue queue;
                    if (_queues.TryRemove(queueName, out queue))
                    {
                        queue.TryDispose();
                    }
                }
            }
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }
}