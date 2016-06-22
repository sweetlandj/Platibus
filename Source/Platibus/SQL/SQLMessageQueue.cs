using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Transactions;
using Common.Logging;
using Platibus.Security;

namespace Platibus.SQL
{
    /// <summary>
    /// A message queue based on a SQL database
    /// </summary>
    public class SQLMessageQueue : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.SQL);

        private readonly IDbConnectionProvider _connectionProvider;
        private readonly ISQLDialect _dialect;
        private readonly QueueName _queueName;
        private readonly IQueueListener _listener;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly bool _autoAcknowledge;
        private readonly int _maxAttempts;
        private readonly ActionBlock<SQLQueuedMessage> _queuedMessages;
        private readonly TimeSpan _retryDelay;

        private bool _disposed;
        private int _initialized;

        /// <summary>
        /// Initializes a new <see cref="SQLMessageQueue"/> with the specified values
        /// </summary>
        /// <param name="connectionProvider">The database connection provider</param>
        /// <param name="dialect">The SQL dialect</param>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="listener">The object that will be notified when messages are
        /// added to the queue</param>
        /// <param name="options">(Optional) Settings that influence how the queue behaves</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connectionProvider"/>,
        /// <paramref name="dialect"/>, <paramref name="queueName"/>, or <paramref name="listener"/>
        /// are <c>null</c></exception>
        public SQLMessageQueue(IDbConnectionProvider connectionProvider, ISQLDialect dialect, QueueName queueName,
            IQueueListener listener, QueueOptions options = default(QueueOptions))
        {
            if (connectionProvider == null) throw new ArgumentNullException("connectionProvider");
            if (dialect == null) throw new ArgumentNullException("dialect");
            if (queueName == null) throw new ArgumentNullException("queueName");
            if (listener == null) throw new ArgumentNullException("listener");

            _connectionProvider = connectionProvider;
            _dialect = dialect;
            _queueName = queueName;

            _listener = listener;
            _autoAcknowledge = options.AutoAcknowledge;

            _maxAttempts = options.MaxAttempts <= 0
                ? QueueOptions.DefaultMaxAttempts
                : options.MaxAttempts;

            _retryDelay = options.RetryDelay <= TimeSpan.Zero
                ? TimeSpan.FromMilliseconds(QueueOptions.DefaultRetryDelay)
                : options.RetryDelay;

            var concurrencyLimit = options.ConcurrencyLimit <= 0
                ? QueueOptions.DefaultConcurrencyLimit
                : options.ConcurrencyLimit;

            _cancellationTokenSource = new CancellationTokenSource();
            _queuedMessages =
                new ActionBlock<SQLQueuedMessage>(msg => ProcessQueuedMessage(msg, _cancellationTokenSource.Token),
                    new ExecutionDataflowBlockOptions
                    {
                        CancellationToken = _cancellationTokenSource.Token,
                        MaxDegreeOfParallelism = concurrencyLimit
                    });
        }

        /// <summary>
        /// Reads previously queued messages from the database and initiates message processing
        /// </summary>
        /// <returns>Returns a task that completes when initialization is complete</returns>
        public async Task Init()
        {
            if (Interlocked.Exchange(ref _initialized, 1) == 0)
            {
                await EnqueueExistingMessages();
            }
        }

        /// <summary>
        /// Adds a message to the queue
        /// </summary>
        /// <param name="message">The message to add</param>
        /// <param name="senderPrincipal">The principal that sent the message or from whom
        /// the message was received</param>
        /// <returns>Returns a task that completes when the message has been added to the queue</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is
        /// <c>null</c></exception>
        /// <exception cref="ObjectDisposedException">Thrown if this SQL message queue instance
        /// has already been disposed</exception>
        public async Task Enqueue(Message message, IPrincipal senderPrincipal)
        {
            if (message == null) throw new ArgumentNullException("message");
            CheckDisposed();

            var queuedMessage = await InsertQueuedMessage(message, senderPrincipal);

            await _queuedMessages.SendAsync(queuedMessage);
            // TODO: handle accepted == false
        }

        /// <summary>
        /// Called by the message processing loop to process an individual message
        /// </summary>
        /// <param name="queuedMessage">The queued message to process</param>
        /// <param name="cancellationToken">A cancellation token that can be used by the caller
        /// to cancel message processing</param>
        /// <returns>Returns a task that completes when the queued message is processed</returns>
        protected async Task ProcessQueuedMessage(SQLQueuedMessage queuedMessage, CancellationToken cancellationToken)
        {
            var messageId = queuedMessage.Message.Headers.MessageId;
            var attemptCount = queuedMessage.Attempts;
            var abandoned = false;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            while (!abandoned && attemptCount < _maxAttempts)
            {
                attemptCount++;

                Log.DebugFormat("Processing queued message {0} (attempt {1} of {2})...", messageId, attemptCount,
                    _maxAttempts);

                var context = new SQLQueuedMessageContext(queuedMessage);
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var message = queuedMessage.Message;
                    await _listener.MessageReceived(message, context, cancellationToken);
                    if (_autoAcknowledge && !context.Acknowledged)
                    {
                        await context.Acknowledge();
                    }
                }
                catch (Exception ex)
                {
                    Log.WarnFormat("Unhandled exception handling queued message {0}", ex, messageId);
                }
                
                if (context.Acknowledged)
                {
                    Log.DebugFormat("Message acknowledged.  Marking message {0} as acknowledged...", messageId);
                    // TODO: Implement journaling
                    await UpdateQueuedMessage(queuedMessage, DateTime.UtcNow, null, attemptCount);
                    Log.DebugFormat("Message {0} acknowledged successfully", messageId);
                    return;
                }
                
                if (attemptCount >= _maxAttempts)
                {
                    Log.WarnFormat("Maximum attempts to proces message {0} exceeded", messageId);
                    abandoned = true;
                }

                if (abandoned)
                {
                    await UpdateQueuedMessage(queuedMessage, null, DateTime.UtcNow, attemptCount);
                    return;
                }

                await UpdateQueuedMessage(queuedMessage, null, null, attemptCount);

                Log.DebugFormat("Message not acknowledged.  Retrying in {0}...", _retryDelay);
                await Task.Delay(_retryDelay, cancellationToken);
            }
        }

        /// <summary>
        /// Inserts a message into the SQL database
        /// </summary>
        /// <param name="message">The message to enqueue</param>
        /// <param name="senderPrincipal">The principal that sent the message or from whom the
        /// message was received</param>
        /// <returns>Returns a task that completes when the message has been inserted into the SQL
        /// database and whose result is a copy of the inserted record</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        protected virtual Task<SQLQueuedMessage> InsertQueuedMessage(Message message, IPrincipal senderPrincipal)
        {
            SQLQueuedMessage queuedMessage;
            var connection = _connectionProvider.GetConnection();
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = _dialect.InsertQueuedMessageCommand;

                        var headers = message.Headers;

                        command.SetParameter(_dialect.MessageIdParameterName, (Guid) headers.MessageId);
                        command.SetParameter(_dialect.QueueNameParameterName, (string) _queueName);
                        command.SetParameter(_dialect.MessageNameParameterName, (string) headers.MessageName);
                        command.SetParameter(_dialect.OriginationParameterName,
                            headers.Origination == null ? null : headers.Origination.ToString());
                        command.SetParameter(_dialect.DestinationParameterName,
                            headers.Destination == null ? null : headers.Destination.ToString());
                        command.SetParameter(_dialect.ReplyToParameterName,
                            headers.ReplyTo == null ? null : headers.ReplyTo.ToString());
                        command.SetParameter(_dialect.ExpiresParameterName, headers.Expires);
                        command.SetParameter(_dialect.ContentTypeParameterName, headers.ContentType);
                        command.SetParameter(_dialect.SenderPrincipalParameterName, SerializePrincipal(senderPrincipal));
                        command.SetParameter(_dialect.HeadersParameterName, SerializeHeaders(headers));
                        command.SetParameter(_dialect.MessageContentParameterName, message.Content);

                        command.ExecuteNonQuery();
                    }
                    scope.Complete();
                }
                queuedMessage = new SQLQueuedMessage(message, senderPrincipal);
            }
            finally
            {
                _connectionProvider.ReleaseConnection(connection);
            }

            // SQL calls are not async to avoid the need for TransactionAsyncFlowOption
            // and dependency on .NET 4.5.1 and later
            return Task.FromResult(queuedMessage);
        }

        /// <summary>
        /// Selects all queued messages from the SQL database
        /// </summary>
        /// <returns>Returns a task that completes when all records have been selected and whose
        /// result is the enumerable sequence of the selected records</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        protected virtual Task<IEnumerable<SQLQueuedMessage>> SelectQueuedMessages()
        {
            var queuedMessages = new List<SQLQueuedMessage>();
            var connection = _connectionProvider.GetConnection();
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = _dialect.SelectQueuedMessagesCommand;
                        command.SetParameter(_dialect.QueueNameParameterName, (string) _queueName);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var messageContent = reader.GetString("MessageContent");
                                var headers = DeserializeHeaders(reader.GetString("Headers"));
                                var senderPrincipal = DeserializePrincipal(reader.GetString("SenderPrincipal"));
                                var message = new Message(headers, messageContent);
                                var attempts = reader.GetInt("Attempts").GetValueOrDefault(0);
                                var queuedMessage = new SQLQueuedMessage(message, senderPrincipal, attempts);
                                queuedMessages.Add(queuedMessage);
                            }
                        }
                    }
                    scope.Complete();
                }
            }
            finally
            {
                _connectionProvider.ReleaseConnection(connection);
            }

            // SQL calls are not async to avoid the need for TransactionAsyncFlowOption
            // and dependency on .NET 4.5.1 and later
            return Task.FromResult(queuedMessages.AsEnumerable());
        }

        /// <summary>
        /// Selects all abandoned messages from the SQL database
        /// </summary>
        /// <returns>Returns a task that completes when all records have been selected and whose
        /// result is the enumerable sequence of the selected records</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        protected virtual Task<IEnumerable<SQLQueuedMessage>> SelectAbandonedMessages(DateTime startDate, DateTime endDate)
        {
            var queuedMessages = new List<SQLQueuedMessage>();
            var connection = _connectionProvider.GetConnection();
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = _dialect.SelectAbandonedMessagesCommand;
                        command.SetParameter(_dialect.QueueNameParameterName, (string)_queueName);
                        command.SetParameter(_dialect.StartDateParameterName, startDate);
                        command.SetParameter(_dialect.EndDateParameterName, endDate);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var messageContent = reader.GetString("MessageContent");
                                var headers = DeserializeHeaders(reader.GetString("Headers"));
                                var senderPrincipal = DeserializePrincipal(reader.GetString("SenderPrincipal"));
                                var message = new Message(headers, messageContent);
                                var attempts = reader.GetInt("Attempts").GetValueOrDefault(0);
                                var queuedMessage = new SQLQueuedMessage(message, senderPrincipal, attempts);
                                queuedMessages.Add(queuedMessage);
                            }
                        }
                    }
                    scope.Complete();
                }
            }
            finally
            {
                _connectionProvider.ReleaseConnection(connection);
            }

            // SQL calls are not async to avoid the need for TransactionAsyncFlowOption
            // and dependency on .NET 4.5.1 and later
            return Task.FromResult(queuedMessages.AsEnumerable());
        }

        /// <summary>
        /// Updates an existing queued message in the SQL database
        /// </summary>
        /// <param name="queuedMessage">The queued message to update</param>
        /// <param name="acknowledged">The date and time the message was acknowledged, if applicable</param>
        /// <param name="abandoned">The date and time the message was abandoned, if applicable</param>
        /// <param name="attempts">The number of attempts to process the message so far</param>
        /// <returns>Returns a task that completes when the record has been updated</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        protected virtual Task UpdateQueuedMessage(SQLQueuedMessage queuedMessage, DateTime? acknowledged,
            DateTime? abandoned, int attempts)
        {
            var connection = _connectionProvider.GetConnection();
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = _dialect.UpdateQueuedMessageCommand;
                        command.SetParameter(_dialect.MessageIdParameterName,
                            (Guid) queuedMessage.Message.Headers.MessageId);
                        command.SetParameter(_dialect.QueueNameParameterName, (string) _queueName);
                        command.SetParameter(_dialect.AcknowledgedParameterName, acknowledged);
                        command.SetParameter(_dialect.AbandonedParameterName, abandoned);
                        command.SetParameter(_dialect.AttemptsParameterName, attempts);

                        command.ExecuteNonQuery();
                    }
                    scope.Complete();
                }
            }
            finally
            {
                _connectionProvider.ReleaseConnection(connection);
            }

            // SQL calls are not async to avoid the need for TransactionAsyncFlowOption
            // and dependency on .NET 4.5.1 and later
            return Task.FromResult(true);
        }

        private async Task EnqueueExistingMessages()
        {
            var queuedMessages = await SelectQueuedMessages();
            foreach (var queuedMessage in queuedMessages)
            {
                Log.DebugFormat("Enqueueing existing message ID {0}...", queuedMessage.Message.Headers.MessageId);
                await _queuedMessages.SendAsync(queuedMessage);
            }
        }

        /// <summary>
        /// Helper method to serialize a principal so that it can be inserted into a single
        /// column in the SQL database
        /// </summary>
        /// <param name="principal">The principal to serialize</param>
        /// <returns>The serialized principal</returns>
        protected virtual string SerializePrincipal(IPrincipal principal)
        {
            if (principal == null) return null;

            var senderPrincipal = new SenderPrincipal(principal);
            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, senderPrincipal);
                var base64String = Convert.ToBase64String(memoryStream.GetBuffer());
                return base64String;
            }
        }

        /// <summary>
        /// A helper method to serialize message headers so that they can be inserted into a
        /// single column in the SQL database
        /// </summary>
        /// <param name="headers">The message headers to serialize</param>
        /// <returns>Returns the serialized message headers</returns>
        protected virtual string SerializeHeaders(IMessageHeaders headers)
        {
            if (headers == null) return null;

            using (var writer = new StringWriter())
            {
                foreach (var header in headers)
                {
                    var headerName = header.Key;
                    var headerValue = header.Value;
                    writer.Write("{0}: ", headerName);
                    using (var headerValueReader = new StringReader(headerValue))
                    {
                        var multilineContinuation = false;
                        string line;
                        while ((line = headerValueReader.ReadLine()) != null)
                        {
                            if (multilineContinuation)
                            {
                                // Prefix continuation with whitespace so that subsequent
                                // lines are not confused with different headers.
                                line = "    " + line;
                            }
                            writer.WriteLine(line);
                            multilineContinuation = true;
                        }
                    }
                }
                return writer.ToString();
            }
        }

        /// <summary>
        /// Helper method to deserialize the sender principal from a record in the SQL database
        /// </summary>
        /// <param name="str">The serialized principal string</param>
        /// <returns>Returns the deserialized principal</returns>
        /// <remarks>
        /// This method performs the inverse of <see cref="SerializePrincipal"/>
        /// </remarks>
        protected virtual IPrincipal DeserializePrincipal(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;

            var bytes = Convert.FromBase64String(str);
            using (var memoryStream = new MemoryStream(bytes))
            {
                var formatter = new BinaryFormatter();
                return (IPrincipal) formatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Helper method to deserialize message headers read from a record in the SQL database
        /// </summary>
        /// <param name="headerString">The serialized header string</param>
        /// <returns>Returns the deserialized headers</returns>
        /// <remarks>
        /// This method performs the inverse of <see cref="SerializeHeaders"/>
        /// </remarks>
        protected virtual IMessageHeaders DeserializeHeaders(string headerString)
        {
            var headers = new MessageHeaders();
            if (string.IsNullOrWhiteSpace(headerString)) return headers;

            var currentHeaderName = (HeaderName) null;
            var currentHeaderValue = new StringWriter();
            var finishedReadingHeaders = false;
            var lineNumber = 0;

            using (var reader = new StringReader(headerString))
            {
                string currentLine;
                while (!finishedReadingHeaders && (currentLine = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    if (string.IsNullOrWhiteSpace(currentLine))
                    {
                        if (currentHeaderName != null)
                        {
                            headers[currentHeaderName] = currentHeaderValue.ToString();
                            currentHeaderName = null;
                            currentHeaderValue = new StringWriter();
                        }

                        finishedReadingHeaders = true;
                        continue;
                    }

                    if (currentLine.StartsWith(" ") && currentHeaderName != null)
                    {
                        // Continuation of previous header value
                        currentHeaderValue.WriteLine();
                        currentHeaderValue.Write(currentLine.Trim());
                        continue;
                    }

                    // New header.  Finish up with the header we were just working on.
                    if (currentHeaderName != null)
                    {
                        headers[currentHeaderName] = currentHeaderValue.ToString();
                        currentHeaderValue = new StringWriter();
                    }

                    if (currentLine.StartsWith("#"))
                    {
                        // Special line. Ignore.
                        continue;
                    }

                    var separatorPos = currentLine.IndexOf(':');
                    if (separatorPos < 0)
                    {
                        throw new FormatException(string.Format("Invalid header on line {0}:  Character ':' expected",
                            lineNumber));
                    }

                    if (separatorPos == 0)
                    {
                        throw new FormatException(
                            string.Format(
                                "Invalid header on line {0}:  Character ':' found at position 0 (missing header name)",
                                lineNumber));
                    }

                    currentHeaderName = currentLine.Substring(0, separatorPos);
                    currentHeaderValue.Write(currentLine.Substring(separatorPos + 1).Trim());
                }

                // Make sure we set the last header we were working on, if there is one
                if (currentHeaderName != null)
                {
                    headers[currentHeaderName] = currentHeaderValue.ToString();
                }
            }

            return headers;
        }

        /// <summary>
        /// Throws <see cref="ObjectDisposedException"/> if this object has been disposed
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if this object has been disposed</exception>
        protected virtual void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        /// <summary>
        /// Finalizer to ensure that all resources are released
        /// </summary>
        ~SQLMessageQueue()
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
        /// Called by the <see cref="Dispose()"/> method or by the finalizer to free held resources
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the 
        /// <see cref="Dispose()"/> method (<c>true</c>) or from the finalizer (<c>false</c>)</param>
        protected virtual void Dispose(bool disposing)
        {
            _cancellationTokenSource.Cancel();
            if (disposing)
            {
				_cancellationTokenSource.TryDispose();
            }
        }
    }
}