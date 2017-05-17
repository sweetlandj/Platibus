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

namespace Platibus.SQL
{
    /// <summary>
    /// A <see cref="ISQLDialect"/> for Microsoft SQL server
    /// </summary>
    public class MSSQLDialect : CommonSQLDialect
    {
        /// <summary>
        /// The MS SQL commands used to create the objects (tables, indexes,
        /// stored procedures, views, etc.) needed to store queued messages in the 
        /// SQL database
        /// </summary>
        public override string CreateMessageQueueingServiceObjectsCommand
        {
            get { return @"
IF OBJECT_ID('[PB_QueuedMessages]') IS NULL
BEGIN
    CREATE TABLE [PB_QueuedMessages]
    (
        [MessageId] UNIQUEIDENTIFIER NOT NULL,
        [QueueName] VARCHAR(255) NOT NULL,
        [MessageName] VARCHAR(500) NULL,
        [Origination] VARCHAR(500) NULL,
        [Destination] VARCHAR(500) NULL,                            
        [ReplyTo] VARCHAR(500) NULL,
        [Expires] DATETIME NULL,
        [ContentType] VARCHAR(100) NULL,
        [SenderPrincipal] VARCHAR(MAX),
        [Headers] VARCHAR(MAX),
        [MessageContent] TEXT,
        [Acknowledged] DATETIME NULL,
        [Abandoned] DATETIME NULL,
        [Attempts] INT NOT NULL DEFAULT 0,

        CONSTRAINT [PB_QueuedMessages_PK] 
            PRIMARY KEY NONCLUSTERED ([MessageId], [QueueName])
    )

    CREATE INDEX [PB_QueuedMessages_IX_QueueName] 
        ON [PB_QueuedMessages]([QueueName])
END"; }
        }

        /// <inheritdoc />
        public override string CreateMessageJournalingServiceObjectsCommand
        {
            get { return @"
IF OBJECT_ID('[PB_MessageJournal]') IS NULL
BEGIN
    CREATE TABLE [PB_MessageJournal]
    (
        [Id] BIGINT IDENTITY(1,1),
        [MessageId] UNIQUEIDENTIFIER NOT NULL,
        [Timestamp] DATETIMEOFFSET NOT NULL,
        [Category] VARCHAR(20) NOT NULL,
        [TopicName] VARCHAR(100) NOT NULL,
        [MessageName] VARCHAR(500) NULL,
        [Origination] VARCHAR(500) NULL,
        [Destination] VARCHAR(500) NULL,                            
        [ReplyTo] VARCHAR(500) NULL,
        [Expires] DATETIME NULL,
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
    ADD [TopicName] VARCHAR(100)
    
    CREATE INDEX [PB_MessageJournal_IX_TopicName] 
        ON [PB_MessageJournal]([TopicName])
END

"; }
        }

        /// <summary>
        /// The MS SQL commands used to create the objects (tables, indexes,
        /// stored procedures, views, etc.) needed to store subscription tracking data 
        /// in the SQL database
        /// </summary>
        public override string CreateSubscriptionTrackingServiceObjectsCommand
        {
            get { return @"
IF OBJECT_ID('[PB_Subscriptions]') IS NULL
BEGIN
    CREATE TABLE [PB_Subscriptions]
    (
        [TopicName] VARCHAR(255) NOT NULL,
        [Subscriber] VARCHAR(500) NOT NULL,
        [Expires] DATETIME NULL,

        CONSTRAINT [PB_Subscriptions_PK] 
            PRIMARY KEY CLUSTERED ([TopicName], [Subscriber])
    )

    CREATE INDEX [PB_Subscriptions_IX_TopicName] 
        ON [PB_Subscriptions]([TopicName])
END"; }
        }
    }
}