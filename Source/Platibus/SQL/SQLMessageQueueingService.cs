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
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Config.Extensibility;
using Platibus.Queueing;
using Platibus.Security;
using Platibus.SQL.Commands;

namespace Platibus.SQL
{
    /// <summary>
    /// A <see cref="IMessageQueueingService"/> implementation that uses a SQL database to store
    /// queued messages
    /// </summary>
    public class SQLMessageQueueingService : AbstractMessageQueueingService<SQLMessageQueue>
    {
        private readonly IDbConnectionProvider _connectionProvider;
        private readonly IMessageQueueingCommandBuilders _commandBuilders;
        private readonly ISecurityTokenService _securityTokenService;

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

        /// <inheritdoc />
        protected override Task<SQLMessageQueue> InternalCreateQueue(QueueName queueName, IQueueListener listener, QueueOptions options = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var queue = new SQLMessageQueue(_connectionProvider, _commandBuilders, queueName, listener, _securityTokenService, options);
            return Task.FromResult(queue);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if(disposing)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                var disposableConnectionProvider = _connectionProvider as IDisposable;
                if (disposableConnectionProvider != null)
                {
                    disposableConnectionProvider.Dispose();
                }
            }
        }
    }
}