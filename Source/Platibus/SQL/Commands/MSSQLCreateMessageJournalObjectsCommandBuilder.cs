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
    /// A subclass of <see cref="CreateMessageJournalObjectsCommandBuilder"/> that produces
    /// commands for creating message journal objects in a SQL Server database using Transact-SQL
    /// syntax.
    /// </summary>
    public class MSSQLCreateMessageJournalObjectsCommandBuilder : CreateMessageJournalObjectsCommandBuilder
    {
        /// <inheritdoc />
        public override DbCommand BuildDbCommand(DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = CommandText;
            return command;
        }

        /// <summary>
        /// The default command text (Transact-SQL syntax)
        /// </summary>
        public virtual string CommandText
        {
            get
            {
                return @"
IF OBJECT_ID('[PB_MessageJournal]') IS NULL
BEGIN
    CREATE TABLE [PB_MessageJournal]
    (
        [Id] BIGINT IDENTITY(1,1),
        [MessageId] UNIQUEIDENTIFIER NOT NULL,
        [Timestamp] DATETIME NOT NULL,
        [Category] VARCHAR(20) NOT NULL,
        [TopicName] VARCHAR(100) NOT NULL,
        [MessageName] VARCHAR(500) NULL,
        [Origination] VARCHAR(500) NULL,
        [Destination] VARCHAR(500) NULL,                            
        [ReplyTo] VARCHAR(500) NULL,
        [ContentType] VARCHAR(100) NULL,
        [Headers] VARCHAR(MAX),
        [MessageContent] TEXT,

        CONSTRAINT [PB_MessageJournal_PK] PRIMARY KEY CLUSTERED ([Id])
    )

    CREATE INDEX [PB_MessageJournal_IX_MessageId] 
        ON [PB_MessageJournal]([MessageId])

    CREATE INDEX [PB_MessageJournal_IX_Timestamp] 
        ON [PB_MessageJournal]([Timestamp])

    CREATE INDEX [PB_MessageJournal_IX_Category] 
        ON [PB_MessageJournal]([Category])
END

IF NOT EXISTS (SELECT * FROM [sys].[columns] 
           WHERE [object_id] = OBJECT_ID(N'[PB_MessageJournal]') 
           AND [name] = 'Timestamp')
BEGIN
    ALTER TABLE [PB_MessageJournal]
    ADD [Timestamp] DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
    
    CREATE INDEX [PB_MessageJournal_IX_Timestamp] 
        ON [PB_MessageJournal]([Timestamp])
END

IF NOT EXISTS (SELECT * FROM [sys].[columns] 
           WHERE [object_id] = OBJECT_ID(N'[PB_MessageJournal]') 
           AND [name] = 'TopicName')
BEGIN
    ALTER TABLE [PB_MessageJournal]
    ADD [Topic] VARCHAR(100)
    
    CREATE INDEX [PB_MessageJournal_IX_TopicName] 
        ON [PB_MessageJournal]([TopicName])
END

";
            }
        }
    }
}