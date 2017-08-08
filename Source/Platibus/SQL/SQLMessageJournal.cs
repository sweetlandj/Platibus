// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
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
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Platibus.Config.Extensibility;
using Platibus.Diagnostics;
using Platibus.Journaling;
using Platibus.SQL.Commands;

namespace Platibus.SQL
{
    /// <summary>
    /// A <see cref="IMessageJournal"/> implementation that uses a SQL database to store journaled
    /// messages
    /// </summary>
    public class SQLMessageJournal : IMessageJournal
    {
        /// <summary>
        /// A data sink provided by the implementer to handle diagnostic events
        /// </summary>
        protected readonly IDiagnosticService DiagnosticService;

        private readonly IDbConnectionProvider _connectionProvider;
        private readonly IMessageJournalingCommandBuilders _commandBuilders;
        
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
        public IMessageJournalingCommandBuilders CommandBuilders
        {
            get { return _commandBuilders; }
        }

        /// <summary>
        /// Initializes the message queueing service by creating the necessary objects in the
        /// SQL database
        /// </summary>
        public void Init()
        {
            var connection = _connectionProvider.GetConnection();
            try
            {
                var createObjectsCommand = _commandBuilders.NewCreateObjectsCommandBuilder();
                using (var command = createObjectsCommand.BuildDbCommand(connection))
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
        /// Initializes a new <see cref="SQLMessageJournal"/> with the specified connection
        /// string settings and dialect
        /// </summary>
        /// <param name="connectionStringSettings">The connection string settings to use to connect
        ///     to the SQL database</param>
        /// <param name="commandBuilders">(Optional) A collection of factories capable of 
        ///     generating database commands for manipulating queued messages that conform to the SQL
        ///     syntax required by the underlying connection provider (if needed)</param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        ///     are reported and processed</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connectionStringSettings"/>
        /// is <c>null</c></exception>
        /// <remarks>
        /// If a SQL dialect is not specified, then one will be selected based on the supplied
        /// connection string settings
        /// </remarks>
        /// <seealso cref="CommandBuildersFactory.InitMessageJournalingCommandBuilders"/>
        /// <seealso cref="IMessageJournalingCommandBuildersProvider"/>
        public SQLMessageJournal(ConnectionStringSettings connectionStringSettings, 
            IMessageJournalingCommandBuilders commandBuilders = null, 
            IDiagnosticService diagnosticService = null)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException("connectionStringSettings");
            DiagnosticService = diagnosticService ?? Diagnostics.DiagnosticService.DefaultInstance;
            _connectionProvider = new DefaultConnectionProvider(connectionStringSettings, DiagnosticService);
           _commandBuilders = commandBuilders ??
                               new CommandBuildersFactory(connectionStringSettings, DiagnosticService)
                                   .InitMessageJournalingCommandBuilders();
        }

        /// <summary>
        /// Initializes a new <see cref="SQLMessageJournal"/> with the specified connection
        /// provider and dialect
        /// </summary>
        /// <param name="connectionProvider">The connection provider to use to connect to
        ///     the SQL database</param>
        /// <param name="commandBuilders">A set of commands that conform to the SQL syntax
        ///     required by the underlying connection provider</param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        ///     are reported and processed</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connectionProvider"/>
        /// or <paramref name="commandBuilders"/> is <c>null</c></exception>
        public SQLMessageJournal(IDbConnectionProvider connectionProvider, 
            IMessageJournalingCommandBuilders commandBuilders, 
            IDiagnosticService diagnosticService = null)
        {
            if (connectionProvider == null) throw new ArgumentNullException("connectionProvider");
            if (commandBuilders == null) throw new ArgumentNullException("commandBuilders");
            DiagnosticService = diagnosticService ?? Diagnostics.DiagnosticService.DefaultInstance;
            _connectionProvider = connectionProvider;
            _commandBuilders = commandBuilders;
        }

        /// <inheritdoc />
        public virtual Task<MessageJournalPosition> GetBeginningOfJournal(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult<MessageJournalPosition>(new SQLMessageJournalPosition(0));
        }

        /// <inheritdoc />
        public virtual async Task Append(Message message, MessageJournalCategory category, CancellationToken cancellationToken = new CancellationToken())
        {
            if (message == null) throw new ArgumentNullException("message");
            if (category == null) throw new ArgumentNullException("category");

            var timestamp = message.GetJournalTimestamp(category);
            var connection = _connectionProvider.GetConnection();
            try
            {
                var headers = message.Headers;
                var commandBuilder = _commandBuilders.NewInsertJournaledMessageCommandBuilder();

                commandBuilder.MessageId = headers.MessageId;
                commandBuilder.Timestamp = timestamp;
                commandBuilder.Category = category;
                commandBuilder.TopicName = headers.Topic;
                commandBuilder.MessageName = headers.MessageName;
                commandBuilder.Origination = headers.Origination == null
                    ? null
                    : headers.Origination.WithTrailingSlash().ToString();
                commandBuilder.Destination = headers.Destination == null
                    ? null
                    : headers.Destination.WithTrailingSlash().ToString();
                commandBuilder.ReplyTo = headers.ReplyTo == null 
                    ? null 
                    : headers.ReplyTo.ToString();
                commandBuilder.RelatedTo = headers.RelatedTo;
                commandBuilder.ContentType = headers.ContentType;
                commandBuilder.Headers = SerializeHeaders(headers);
                commandBuilder.Content = message.Content;

                using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    using (var command = commandBuilder.CreateDbCommand(connection))
                    {
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    scope.Complete();
                }
            }
            finally
            {
                _connectionProvider.ReleaseConnection(connection);
            }
        }

        /// <inheritdoc />
        public virtual async Task<MessageJournalReadResult> Read(MessageJournalPosition start, int count, MessageJournalFilter filter = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var myFilter = filter ?? new MessageJournalFilter();
            var next = start;
            var journaledMessages = new List<MessageJournalEntry>();
            var endOfJournal = true;
            var connection = _connectionProvider.GetConnection();
            try
            {
                var commandBuilder = _commandBuilders.NewSelectJournaledMessagesCommandBuilder();
                commandBuilder.Categories = myFilter.Categories.Select(c => (string) c).ToList();
                commandBuilder.Topics = myFilter.Topics.Select(t => (string) t).ToList();
                commandBuilder.From = myFilter.From;
                commandBuilder.To = myFilter.To;
                commandBuilder.Origination = myFilter.Origination;
                commandBuilder.Destination = myFilter.Destination;
                commandBuilder.RelatedTo = myFilter.RelatedTo;
                commandBuilder.MessageName = myFilter.MessageName;
                commandBuilder.Start = ((SQLMessageJournalPosition) start).Id;
                commandBuilder.Count = count + 1;

                using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    using (var command = commandBuilder.BuildDbCommand(connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                var record = commandBuilder.BuildJournaledMessageRecord(reader);
                                next = new SQLMessageJournalPosition(record.Id);
                                if (journaledMessages.Count < count)
                                {
                                    var category = record.Category;
                                    var timestamp = record.Timestamp;
                                    var headers = DeserializeHeaders(record.Headers);
                                    var messageContent = record.Content;
                                    var offset = new SQLMessageJournalPosition(record.Id);
                                    var message = new Message(headers, messageContent);
                                    var journaledMessage = new MessageJournalEntry(category, offset, timestamp, message);
                                    journaledMessages.Add(journaledMessage);

                                    next = new SQLMessageJournalPosition(record.Id + 1);
                                }
                                else
                                {
                                    endOfJournal = false;
                                }
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

            return new MessageJournalReadResult(start, next, endOfJournal, journaledMessages);
        }

        /// <inheritdoc />
        public virtual MessageJournalPosition ParsePosition(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) throw new ArgumentNullException("str");
            return new SQLMessageJournalPosition(long.Parse(str));
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