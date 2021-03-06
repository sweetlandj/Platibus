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
#if NET452 || NET461
using System.Configuration;
#endif
#if NETSTANDARD2_0
using Platibus.Config;
#endif
using System.Threading;
using System.Threading.Tasks;
using Platibus.Diagnostics;
using Platibus.Queueing;
using Platibus.Security;
using Platibus.SQL.Commands;

namespace Platibus.SQL
{
    /// <inheritdoc />
    /// <summary>
    /// A <see cref="T:Platibus.IMessageQueueingService" /> implementation that uses a SQL database to store
    /// queued messages
    /// </summary>
    public class SQLMessageQueueingService : AbstractMessageQueueingService<SQLMessageQueue>
    {
        /// <summary>
        /// A data sink provided by the implementer to handle diagnostic events
        /// </summary>
        protected readonly IDiagnosticService DiagnosticService;

        private readonly ISecurityTokenService _securityTokenService;
        private readonly IMessageEncryptionService _messageEncryptionService;

        /// <summary>
        /// The connection provider used to obtain connections to the SQL database
        /// </summary>
        public IDbConnectionProvider ConnectionProvider { get; }

        /// <summary>
        /// The SQL dialect
        /// </summary>
        public IMessageQueueingCommandBuilders CommandBuilders { get; }

        /// <summary>
        /// Initializes a new <see cref="SQLMessageQueueingService"/> with the specified connection
        /// string settings and dialect
        /// </summary>
        /// <param name="options">Options influencing the behavior of this service</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/>
        /// is <c>null</c></exception>
        /// <remarks>
        /// <para>If a SQL dialect is not specified, then one will be selected based on the 
        /// supplied connection string settings</para>
        /// <para>If a security token service is not specified then a default implementation 
        /// based on unsigned JWTs will be used.</para>
        /// </remarks>
        /// <seealso cref="Platibus.Config.Extensibility.IMessageQueueingCommandBuildersProvider"/>
        public SQLMessageQueueingService(SQLMessageQueueingOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            DiagnosticService = options.DiagnosticService ?? Diagnostics.DiagnosticService.DefaultInstance;
            ConnectionProvider = options.ConnectionProvider;
            CommandBuilders = options.CommandBuilders;
            _securityTokenService = options.SecurityTokenService ?? new JwtSecurityTokenService();
            _messageEncryptionService = options.MessageEncryptionService;
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.SQL.SQLMessageQueueingService" /> with the specified connection
        /// string settings and dialect
        /// </summary>
        /// <param name="connectionStringSettings">The connection string settings to use to connect
        ///     to the SQL database</param>
        /// <param name="commandBuilders">(Optional) A collection of factories capable of 
        ///     generating database commands for manipulating queued messages that conform to the 
        ///     SQL syntax required by the underlying connection provider (if needed)</param>
        /// <param name="securityTokenService">(Optional) The message security token
        ///     service to use to issue and validate security tokens for persisted messages.</param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        ///     are reported and processed</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown if <paramref name="connectionStringSettings" />
        /// is <c>null</c></exception>
        /// <remarks>
        /// <para>If a SQL dialect is not specified, then one will be selected based on the 
        /// supplied connection string settings</para>
        /// <para>If a <paramref name="securityTokenService" /> is not specified then a
        /// default implementation based on unsigned JWTs will be used.</para>
        /// </remarks>
        /// <seealso cref="T:Platibus.Config.Extensibility.IMessageQueueingCommandBuildersProvider" />
        [Obsolete]
        public SQLMessageQueueingService(ConnectionStringSettings connectionStringSettings,
            IMessageQueueingCommandBuilders commandBuilders = null,
            ISecurityTokenService securityTokenService = null,
            IDiagnosticService diagnosticService = null)
            : this(new SQLMessageQueueingOptions(new DefaultConnectionProvider(connectionStringSettings),
                commandBuilders)
            {
                DiagnosticService = diagnosticService,
                SecurityTokenService = securityTokenService
            })
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.SQL.SQLMessageQueueingService" /> with the specified connection
        /// provider and dialect
        /// </summary>
        /// <param name="connectionProvider">The connection provider to use to connect to the SQL 
        ///     database</param>
        /// <param name="commandBuilders">A collection of factories capable of  generating database
        ///     commands for manipulating queued messages that conform to the SQL syntax required 
        ///     by the underlying connection provider</param>
        /// <param name="securityTokenService">(Optional) The message security token
        ///     service to use to issue and validate security tokens for persisted messages.</param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        ///     are reported and processed</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown if <paramref name="connectionProvider" />
        ///     or <paramref name="commandBuilders" /> is <c>null</c></exception>
        /// <remarks>
        /// <para>If a <paramref name="securityTokenService" /> is not specified then a
        ///     default implementation based on unsigned JWTs will be used.</para>
        /// </remarks>
        /// <seealso cref="T:Platibus.Config.Extensibility.IMessageQueueingCommandBuildersProvider" />
        [Obsolete]
        public SQLMessageQueueingService(IDbConnectionProvider connectionProvider, 
            IMessageQueueingCommandBuilders commandBuilders, 
            ISecurityTokenService securityTokenService = null, 
            IDiagnosticService diagnosticService = null)
            : this(new SQLMessageQueueingOptions(connectionProvider, commandBuilders)
            {
                DiagnosticService = diagnosticService,
                SecurityTokenService = securityTokenService
            })
        {
        }

        /// <summary>
        /// Initializes the message queueing service by creating the necessary objects in the
        /// SQL database
        /// </summary>
        public void Init()
        {
            var connection = ConnectionProvider.GetConnection();
            try
            {
                var commandBuilder = CommandBuilders.NewCreateObjectsCommandBuilder();
                using (var command = commandBuilder.BuildDbCommand(connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                ConnectionProvider.ReleaseConnection(connection);
            }
        }

        /// <inheritdoc />
        protected override Task<SQLMessageQueue> InternalCreateQueue(QueueName queueName, IQueueListener listener,
            QueueOptions options = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var queue = new SQLMessageQueue(queueName, listener, options, DiagnosticService, ConnectionProvider,
                CommandBuilders, _securityTokenService, _messageEncryptionService);
            return Task.FromResult(queue);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (ConnectionProvider is IDisposable disposableConnectionProvider)
                {
                    disposableConnectionProvider.Dispose();
                }
            }
        }
    }
}