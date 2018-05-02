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
using System.Data;
using System.Data.Common;

namespace Platibus.SQL.Commands
{
    /// <summary>
    /// Default command builder for creating commands to insert new journaled messages into the
    /// database.
    /// </summary>
    public class InsertMessageJournalEntryCommandBuilder
    {
        /// <summary>
        /// The message ID
        /// </summary>
        public Guid MessageId { get; set; }

        /// <summary>
        /// The timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The journal category
        /// </summary>
        /// <see cref="Platibus.Journaling.MessageJournalCategory"/>
        public string Category { get; set; }

        /// <summary>
        /// The name of the topic to which the message was published (if applicable)
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// The message name
        /// </summary>
        public string MessageName { get; set; }

        /// <summary>
        /// The URI of the instance from which the message originated
        /// </summary>
        public string Origination { get; set; }

        /// <summary>
        /// The URI of the instance to which the message is addressed
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// The ID of the message to which this message is related (e.g. reply)
        /// </summary>
        public Guid RelatedTo { get; set; }

        /// <summary>
        /// The URI of the instance to which replies to the message should be addressed
        /// </summary>
        public string ReplyTo { get; set; }

        /// <summary>
        /// The MIME type of the message content
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Message headers concatenated and formatted as a single string value
        /// </summary>
        public string Headers { get; set; }

        /// <summary>
        /// The message content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Initializes and returns a new non-query <see cref="DbCommand"/> with the configured
        /// parameters
        /// </summary>
        /// <param name="connection">An open database connection</param>
        /// <returns>Returns a new non-query <see cref="DbCommand"/> with the configured
        /// parameters</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual DbCommand CreateDbCommand(DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = CommandText;

            command.SetParameter("@MessageId", MessageId);
            command.SetParameter("@Timestamp", Timestamp);
            command.SetParameter("@Category", Category);
            command.SetParameter("@TopicName", TopicName);
            command.SetParameter("@MessageName", MessageName);
            command.SetParameter("@Origination", Origination);
            command.SetParameter("@Destination", Destination);
            command.SetParameter("@ReplyTo", ReplyTo);
            command.SetParameter("@RelatedTo", RelatedTo);
            command.SetParameter("@ContentType", ContentType);
            command.SetParameter("@Headers", Headers);
            command.SetParameter("@MessageContent", Content);

            return command;
        }

        /// <summary>
        /// The default command text (Transact-SQL syntax)
        /// </summary>
        public virtual string CommandText => @"
INSERT INTO [PB_MessageJournal] (
    [MessageId],
    [Timestamp],
    [Category],
    [TopicName],
    [MessageName], 
    [Origination], 
    [Destination], 
    [ReplyTo],
    [RelatedTo], 
    [ContentType], 
    [Headers], 
    [MessageContent])
VALUES ( 
    @MessageId, 
    @Timestamp,
    @Category,
    @TopicName,
    @MessageName, 
    @Origination, 
    @Destination, 
    @ReplyTo, 
    @RelatedTo,
    @ContentType, 
    @Headers, 
    @MessageContent)";
    }
}
