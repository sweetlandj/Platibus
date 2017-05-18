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
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Platibus.Config.Extensibility;
using Platibus.Security;
using Platibus.SQL.Commands;

namespace Platibus.SQL
{
    /// <summary>
    /// A <see cref="IMessageQueueingService"/> implementation that uses a SQL database to store
    /// queued messages
    /// </summary>
    public class SQLMessageQueueingService : IMessageQueueingService, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.SQL);

        private readonly IDbConnectionProvider _connectionProvider;
        private readonly IMessageQueueingCommandBuilders _commandBuilders;
        private readonly ISecurityTokenService _securityTokenService;

        private readonly ConcurrentDictionary<QueueName, SQLMessageQueue> _queues =
            new ConcurrentDictionary<QueueName, SQLMessageQueue>();

        private bool _disposed;

        /// <summary>
        /// The connection provider used to obtain connections to the SQL database
        /// </summary>
        public IDbConnectionProvider ConnectionProvider
        {
            get { return _connectionProvider; }
        }

        /// <summary>
        /// The SQL dialect
        /// </summary>
        public IMessageQueueingCommandBuilders CommandBuilders
        {
            get { return _commandBuilders; }
        }

        /// <summary>
        /// Initializes a new <see cref="SQLMessageQueueingService"/> with the specified connection
        /// string settings and dialect
        /// </summary>
        /// <param name="connectionStringSettings">The connection string settings to use to connect to
        /// the SQL database</param>
        /// <param name="commandBuilders">(Optional) A collection of factories capable of 
        /// generating database commands for manipulating queued messages that conform to the SQL
        /// syntax required by the underlying connection provider (if needed)</param>
        /// <param name="securityTokenService">(Optional) The message security token
        /// service to use to issue and validate security tokens for persisted messages.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connectionStringSettings"/>
        /// is <c>null</c></exception>
        /// <remarks>
        /// <para>If a SQL dialect is not specified, then one will be selected based on the supplied
        /// connection string settings</para>
        /// <para>If a <paramref name="securityTokenService"/> is not specified then a
        /// default implementation based on unsigned JWTs will be used.</para>
        /// </remarks>
        /// <seealso cref="CommandBuilderExtensions.GetMessageQueueingCommandBuilders"/>
        /// <seealso cref="IMessageQueueingCommandBuildersProvider"/>
        public SQLMessageQueueingService(ConnectionStringSettings connectionStringSettings, IMessageQueueingCommandBuilders commandBuilders = null, ISecurityTokenService securityTokenService = null)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException("connectionStringSettings");
            _connectionProvider = new DefaultConnectionProvider(connectionStringSettings);
            _commandBuilders = commandBuilders ?? connectionStringSettings.GetMessageQueueingCommandBuilders();
            _securityTokenService = securityTokenService ?? new JwtSecurityTokenService();
        }

        /// <summary>
        /// Initializes a new <see cref="SQLMessageQueueingService"/> with the specified connection
        /// provider and dialect
        /// </summary>
        /// <param name="connectionProvider">The connection provider to use to connect to
        /// the SQL database</param>
        /// <param name="commandBuilders">A collection of factories capable of  generating database
        /// commands for manipulating queued messages that conform to the SQL syntax required by 
        /// the underlying connection provider</param>
        /// <param name="securityTokenService">(Optional) The message security token
        /// service to use to issue and validate security tokens for persisted messages.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connectionProvider"/>
        /// or <paramref name="commandBuilders"/> is <c>null</c></exception>
        /// <remarks>
        /// <para>If a <paramref name="securityTokenService"/> is not specified then a
        /// default implementation based on unsigned JWTs will be used.</para>
        /// </remarks>
        public SQLMessageQueueingService(IDbConnectionProvider connectionProvider, IMessageQueueingCommandBuilders commandBuilders, ISecurityTokenService securityTokenService = null)
        {
            if (connectionProvider == null) throw new ArgumentNullException("connectionProvider");
            if (commandBuilders == null) throw new ArgumentNullException("commandBuilders");
            _connectionProvider = connectionProvider;
            _commandBuilders = commandBuilders;
            _securityTokenService = securityTokenService ?? new JwtSecurityTokenService();
        }

        /// <summary>
        /// Initializes the message queueing service by creating the necessary objects in the
        /// SQL database
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public void Init()
        {
            var connection = _connectionProvider.GetConnection();
            try
            {
                var commandBuilder = _commandBuilders.NewCreateObjectsCommandBuilder();
                using (var command = commandBuilder.BuildDbCommand(connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                _connectionProvider.ReleaseConnection(connection);
            }
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
        /// <exception cref="ObjectDisposedException">Thrown if the object is already disposed</exception>
        public async Task CreateQueue(QueueName queueName, IQueueListener listener, QueueOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            var queue = new SQLMessageQueue(_connectionProvider, _commandBuilders, queueName, listener, _securityTokenService, options);
            if (!_queues.TryAdd(queueName, queue))
            {
                throw new QueueAlreadyExistsException(queueName);
            }

            Log.DebugFormat("Initializing SQL queue named \"{0}\"...", queueName);
            await queue.Init();
            Log.DebugFormat("SQL queue \"{0}\" created successfully", queueName);
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
        /// <exception cref="ObjectDisposedException">Thrown if the object is already disposed</exception>
        public async Task EnqueueMessage(QueueName queueName, Message message, IPrincipal senderPrincipal, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            SQLMessageQueue queue;
            if (!_queues.TryGetValue(queueName, out queue)) throw new QueueNotFoundException(queueName);

            Log.DebugFormat("Enqueueing message ID {0} in SQL queue \"{1}\"...", message.Headers.MessageId, queueName);
            await queue.Enqueue(message, senderPrincipal);
            Log.DebugFormat("Message ID {0} enqueued successfully in SQL queue \"{1}\"", message.Headers.MessageId,
                queueName);
        }

        /// <summary>
        /// Finalizer that ensures that all resources are released
        /// </summary>
        ~SQLMessageQueueingService()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        /// <summary>
        /// Called by the <see cref="Dispose()"/> method or by the finalizer to free held resources
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the 
        /// <see cref="Dispose()"/> method (<c>true</c>) or from the finalizer (<c>false</c>)</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                foreach (var queue in _queues.Values)
                {
                    queue.TryDispose();
                }

                _connectionProvider.TryDispose();
            }
        }

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if the message queueing service has
        /// been disposed
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object is already disposed</exception>
        protected void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }
}