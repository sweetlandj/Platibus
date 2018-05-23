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
    /// A subclass of <see cref="CreateMessageQueueingObjectsCommandBuilder"/> that produces
    /// commands for creating message queueing objects in a SQL Server database using Transact-SQL
    /// syntax.
    /// </summary>
    public class MSSQLCreateMessageQueueingObjectsCommandBuilder : CreateMessageQueueingObjectsCommandBuilder
    {
        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public override DbCommand BuildDbCommand(DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = CommandText;
            return command;
        }

        /// <summary>
        /// The default command text (Transact-SQL syntax)
        /// </summary>
        public virtual string CommandText => @"
IF OBJECT_ID('[PB_QueuedMessages]') IS NULL
BEGIN
    CREATE TABLE [PB_QueuedMessages]
    (
        [MessageId] UNIQUEIDENTIFIER NOT NULL,
        [QueueName] NVARCHAR(255) NOT NULL,
        [MessageName] NVARCHAR(500) NULL,
        [Origination] NVARCHAR(500) NULL,
        [Destination] NVARCHAR(500) NULL,                            
        [ReplyTo] NVARCHAR(500) NULL,
        [Expires] DATETIME NULL,
        [ContentType] VARCHAR(100) NULL,
        [SenderPrincipal] NVARCHAR(MAX),
        [Headers] NVARCHAR(MAX),
        [MessageContent] NVARCHAR(MAX),
        [Acknowledged] DATETIME NULL,
        [Abandoned] DATETIME NULL,
        [Attempts] INT NOT NULL DEFAULT 0,

        CONSTRAINT [PB_QueuedMessages_PK] 
            PRIMARY KEY NONCLUSTERED ([MessageId], [QueueName])
    )

    CREATE INDEX [PB_QueuedMessages_IX_QueueName] 
        ON [PB_QueuedMessages]([QueueName])
END";
    }
}
