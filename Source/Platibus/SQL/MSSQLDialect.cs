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

    CREATE CLUSTERED INDEX [PB_QueuedMessages_IX_QueueName] 
        ON [PB_QueuedMessages]([QueueName])
END"; }
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