using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Platibus.SQL
{
    /// <summary>
    /// A <see cref="IMessageJournalingService"/> implementation that uses a SQL database 
    /// to store journaled messages
    /// </summary>
    public class SQLMessageJournalingService : IMessageJournalingService
    {
        private readonly IDbConnectionProvider _connectionProvider;
        private readonly ISQLDialect _dialect;

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
        public ISQLDialect Dialect
        {
            get { return _dialect; }
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
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = _dialect.CreateMessageJournalingServiceObjectsCommand;
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                _connectionProvider.ReleaseConnection(connection);
            }
        }

        /// <summary>
        /// Initializes a new <see cref="SQLMessageJournalingService"/> with the specified connection
        /// string settings and dialect
        /// </summary>
        /// <param name="connectionStringSettings">The connection string settings to use to connect to
        /// the SQL database</param>
        /// <param name="dialect">(Optional) The SQL dialect to use</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connectionStringSettings"/>
        /// is <c>null</c></exception>
        /// <remarks>
        /// If a SQL dialect is not specified, then one will be selected based on the supplied
        /// connection string settings
        /// </remarks>
        /// <seealso cref="DbExtensions.GetSQLDialect"/>
        /// <seealso cref="ISQLDialectProvider"/>
        public SQLMessageJournalingService(ConnectionStringSettings connectionStringSettings, ISQLDialect dialect = null)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException("connectionStringSettings");
            _connectionProvider = new DefaultConnectionProvider(connectionStringSettings);
            _dialect = dialect ?? connectionStringSettings.GetSQLDialect();
        }

        /// <summary>
        /// Initializes a new <see cref="SQLMessageJournalingService"/> with the specified connection
        /// provider and dialect
        /// </summary>
        /// <param name="connectionProvider">The connection provider to use to connect to
        /// the SQL database</param>
        /// <param name="dialect">The SQL dialect to use</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connectionProvider"/>
        /// or <paramref name="dialect"/> is <c>null</c></exception>
        public SQLMessageJournalingService(IDbConnectionProvider connectionProvider, ISQLDialect dialect)
        {
            if (connectionProvider == null) throw new ArgumentNullException("connectionProvider");
            if (dialect == null) throw new ArgumentNullException("dialect");
            _connectionProvider = connectionProvider;
            _dialect = dialect;
        }

        /// <inheritdoc />
        public Task MessageReceived(Message message, CancellationToken cancellationToken = new CancellationToken())
        {
            var received = message.Headers.Received;
            if (received == default(DateTime))
            {
                received = DateTime.UtcNow;
            }
            return InsertJournaledMessage(message, "Received", received);
        }

        /// <inheritdoc />
        public Task MessageSent(Message message, CancellationToken cancellationToken = new CancellationToken())
        {
            var sent = message.Headers.Sent;
            if (sent == default(DateTime))
            {
                sent = DateTime.UtcNow;
            }
            return InsertJournaledMessage(message, "Sent", sent);
        }

        /// <inheritdoc />
        public Task MessagePublished(Message message, CancellationToken cancellationToken = new CancellationToken())
        {
            var published = message.Headers.Published;
            if (published == default(DateTime))
            {
                published = DateTime.UtcNow;
            }
            return InsertJournaledMessage(message, "Published", published);
        }

        /// <summary>
        /// Selects all journaled messages from the SQL database
        /// </summary>
        /// <returns>Returns a task that completes when all records have been selected and whose
        /// result is the enumerable sequence of the selected records</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        protected virtual Task<IEnumerable<SQLJournaledMessage>> SelectJournaledMessages()
        {
            var queuedMessages = new List<SQLJournaledMessage>();
            var connection = _connectionProvider.GetConnection();
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = _dialect.SelectJournaledMessagesCommand;

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var messageContent = reader.GetString("MessageContent");
                                var category = reader.GetString("Category");
                                var headers = DeserializeHeaders(reader.GetString("Headers"));
                                var message = new Message(headers, messageContent);
                                var queuedMessage = new SQLJournaledMessage(message, category);
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
        /// Inserts a journaled message into the SQL database
        /// </summary>
        /// <param name="message">The message to enqueue</param>
        /// <param name="category">The journaled message category, e.g. "Sent", "Received", or "Published"</param>
        /// <param name="timestamp">(Optional) The date/time that the message was sent, received, or published 
        /// according to message headers</param>
        /// <returns>Returns a task that completes when the message has been inserted into the SQL
        /// database and whose result is a copy of the inserted record</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        protected virtual Task<SQLJournaledMessage> InsertJournaledMessage(Message message, string category, DateTimeOffset timestamp = default(DateTimeOffset))
        {
            if (message == null) throw new ArgumentNullException("message");
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentNullException("category");

            if (timestamp == default(DateTimeOffset))
            {
                timestamp = DateTimeOffset.UtcNow;
            }

            SQLJournaledMessage journaledMessage;
            var connection = _connectionProvider.GetConnection();
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = _dialect.InsertJournaledMessageCommand;

                        var headers = message.Headers;

                        command.SetParameter(_dialect.MessageIdParameterName, (Guid)headers.MessageId);
                        command.SetParameter(_dialect.TimestampParameterName, timestamp);
                        command.SetParameter(_dialect.CategoryParameterName, category);
                        command.SetParameter(_dialect.TopicNameParameterName, category);
                        command.SetParameter(_dialect.MessageNameParameterName, (string)headers.MessageName);
                        command.SetParameter(_dialect.OriginationParameterName,
                            headers.Origination == null ? null : headers.Origination.ToString());
                        command.SetParameter(_dialect.DestinationParameterName,
                            headers.Destination == null ? null : headers.Destination.ToString());
                        command.SetParameter(_dialect.ReplyToParameterName,
                            headers.ReplyTo == null ? null : headers.ReplyTo.ToString());
                        command.SetParameter(_dialect.ExpiresParameterName, headers.Expires);
                        command.SetParameter(_dialect.ContentTypeParameterName, headers.ContentType);
                        command.SetParameter(_dialect.HeadersParameterName, SerializeHeaders(headers));
                        command.SetParameter(_dialect.MessageContentParameterName, message.Content);

                        command.ExecuteNonQuery();
                    }
                    scope.Complete();
                }
                journaledMessage = new SQLJournaledMessage(message, category);
            }
            finally
            {
                _connectionProvider.ReleaseConnection(connection);
            }

            // SQL calls are not async to avoid the need for TransactionAsyncFlowOption
            // and dependency on .NET 4.5.1 and later
            return Task.FromResult(journaledMessage);
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

            var currentHeaderName = (HeaderName)null;
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
    }
}
