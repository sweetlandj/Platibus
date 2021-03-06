﻿// The MIT License (MIT)
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
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Diagnostics;
using Platibus.Security;

namespace Platibus.RabbitMQ
{
    /// <inheritdoc cref="IMessageQueueingService" />
    /// <inheritdoc cref="IDisposable" />
    /// <summary>
    /// An <see cref="T:Platibus.IMessageQueueingService" /> implementation based on RabbitMQ
    /// </summary>
    public class RabbitMQMessageQueueingService : IMessageQueueingService, IDisposable
    {
        private readonly ConcurrentDictionary<QueueName, RabbitMQQueue> _queues =
            new ConcurrentDictionary<QueueName, RabbitMQQueue>();

        private readonly Encoding _encoding;
        private readonly Uri _uri;
        private readonly QueueOptions _defaultQueueOptions;
        private readonly IConnectionManager _connectionManager;
        private readonly ISecurityTokenService _securityTokenService;
        private readonly bool _disposeConnectionManager;
        private readonly IDiagnosticService _diagnosticService;
        private readonly IMessageEncryptionService _messageEncryptionService;

        private bool _disposed;

        /// <summary>
        /// Initializes a new <see cref="RabbitMQMessageQueueingService"/>
        /// </summary>
        /// <param name="options">Options influencing the configuration and behavior of the service</param>
        /// <remarks>
        /// <para>If a security token service is not specified then a default implementation based on
        /// unsigned JWTs will be used.</para>
        /// </remarks>
        public RabbitMQMessageQueueingService(RabbitMQMessageQueueingOptions options)
        {
            _uri = options.Uri;
            _defaultQueueOptions = options.DefaultQueueOptions ?? new QueueOptions();

            var myConnectionManager = options.ConnectionManager;
            if (myConnectionManager == null)
            {
                myConnectionManager = new ConnectionManager();
                _disposeConnectionManager = true;
            }
            _connectionManager = myConnectionManager;
            _encoding = options.Encoding ?? Encoding.UTF8;
            _securityTokenService = options.SecurityTokenService ?? new JwtSecurityTokenService();
            _diagnosticService = options.DiagnosticService ?? DiagnosticService.DefaultInstance;
            _messageEncryptionService = options.MessageEncryptionService;
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.RabbitMQ.RabbitMQMessageQueueingService" />
        /// </summary>
        /// <param name="uri">The URI of the RabbitMQ server</param>
        /// <param name="defaultQueueOptions">(Optional) Default options for queues</param>
        /// <param name="connectionManager">(Optional) The connection manager</param>
        /// <param name="encoding">(Optional) The encoding to use for converting serialized
        /// message content to byte streams</param>
        /// <param name="securityTokenService">(Optional) The message security token
        /// service to use to issue and validate security tokens for persisted messages.</param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown if <paramref name="uri" /> is
        /// <c>null</c></exception>
        /// <remarks>
        /// <para>If a <paramref name="securityTokenService" /> is not specified then a
        /// default implementation based on unsigned JWTs will be used.</para>
        /// </remarks>
        [Obsolete]
        public RabbitMQMessageQueueingService(Uri uri, QueueOptions defaultQueueOptions = null, 
            IConnectionManager connectionManager = null, Encoding encoding = null, 
            ISecurityTokenService securityTokenService = null, 
            IDiagnosticService diagnosticService = null)
        : this(new RabbitMQMessageQueueingOptions(uri)
        {
            DiagnosticService = diagnosticService,
            ConnectionManager = connectionManager,
            Encoding = encoding,
            DefaultQueueOptions = defaultQueueOptions,
            SecurityTokenService = securityTokenService
        })
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Establishes a named queue
        /// </summary>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="listener">An object that will receive messages off of the queue for processing</param>
        /// <param name="options">(Optional) Options that govern how the queue behaves</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used
        /// by the caller to cancel queue creation if necessary</param>
        /// <returns>Returns a task that will complete when the queue has been created</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if <paramref name="queueName" /> or
        /// <paramref name="listener" /> is <c>null</c></exception>
        /// <exception cref="T:Platibus.QueueAlreadyExistsException">Thrown if a queue with the specified
        /// name already exists</exception>
        public Task CreateQueue(QueueName queueName, IQueueListener listener, QueueOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            var connection = _connectionManager.GetConnection(_uri);
            var queue = new RabbitMQQueue(connection, queueName,
                listener, _encoding, options ?? _defaultQueueOptions, _diagnosticService, _securityTokenService, _messageEncryptionService);

            if (!_queues.TryAdd(queueName, queue))
            {
                throw new QueueAlreadyExistsException(queueName);
            }
            
            queue.Init();
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        /// <summary>
        /// Enqueues a message on a queue
        /// </summary>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="message">The message to enqueue</param>
        /// <param name="senderPrincipal">(Optional) The sender principal, if applicable</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be
        /// used be the caller to cancel the enqueue operation if necessary</param>
        /// <returns>Returns a task that will complete when the message has been enqueued</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if <paramref name="queueName" />
        /// or <paramref name="message" /> is <c>null</c></exception>
        public async Task EnqueueMessage(QueueName queueName, Message message, IPrincipal senderPrincipal, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            if (!_queues.TryGetValue(queueName, out var queue)) throw new QueueNotFoundException(queueName);

            await queue.Enqueue(message, senderPrincipal);
        }

        /// <summary>
        /// Deletes the specified queue and its underlying RabbitMQ objects
        /// </summary>
        /// <param name="queueName">The name of the queue to delete</param>
        public void DeleteQueue(QueueName queueName)
        {
            CheckDisposed();
            if (!_queues.TryRemove(queueName, out var queue)) return;

            queue.Delete();
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        /// <summary>
        /// Finalizer that ensures all resources are released
        /// </summary>
        ~RabbitMQMessageQueueingService()
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
        /// Called by the <see cref="Dispose()"/> method or finalizer to ensure that
        /// resources are released
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the 
        /// <see cref="Dispose()"/> method (<c>true</c>) or the finalizer (<c>false</c>)</param>
        /// <remarks>
        /// This method will not be called more than once
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                foreach (var queue in _queues.Values)
                {
                    queue.Dispose();
                }

                if (_disposeConnectionManager && _connectionManager is IDisposable disposableConnectionManager)
                {
                    disposableConnectionManager.Dispose();
                }
            }
            _disposed = true;
        }
    }
}