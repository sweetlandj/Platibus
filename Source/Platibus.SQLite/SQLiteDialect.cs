// The MIT License (MIT)
// 
// Copyright (c) 2015 Jesse Sweetland
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

using Platibus.SQL;

namespace Platibus.SQLite
{
    internal class SQLiteDialect : CommonSQLDialect
    {
        public override string CreateMessageQueueingServiceObjectsCommand
        {
            get { return @"
CREATE TABLE IF NOT EXISTS [PB_QueuedMessages]
(
    [MessageId] TEXT NOT NULL,
    [QueueName] TEXT NOT NULL,
    [MessageName] TEXT NULL,
    [Origination] TEXT NULL,
    [Destination] TEXT NULL,
    [ReplyTo] TEXT NULL,
    [Expires] TEXT NULL,
    [ContentType] TEXT NULL,
    [SenderPrincipal] TEXT,
    [Headers] TEXT,
    [MessageContent] TEXT,
    [Attempts] INT NOT NULL DEFAULT 0,
    [Acknowledged] TEXT NULL,
    [Abandoned] TEXT NULL,
                
    CONSTRAINT [PB_QueuedMessages_PK] 
        PRIMARY KEY ([MessageId], [QueueName])
);

CREATE INDEX IF NOT EXISTS [PB_QueuedMessages_IX_QueueName] 
    ON [PB_QueuedMessages]([QueueName]);"; }
        }

        public override string CreateSubscriptionTrackingServiceObjectsCommand
        {
            get { return @"
CREATE TABLE IF NOT EXISTS [PB_Subscriptions]
(
    [TopicName] TEXT NOT NULL,
    [Subscriber] TEXT NOT NULL,
    [Expires] TEXT NULL,

    CONSTRAINT [PB_Subscriptions_PK] 
        PRIMARY KEY ([TopicName], [Subscriber])
);

CREATE INDEX IF NOT EXISTS [PB_Subscriptions_IX_TopicName] 
    ON [PB_Subscriptions]([TopicName]);"; }
        }
    }
}