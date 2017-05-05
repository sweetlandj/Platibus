﻿using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Platibus.SQLite;

namespace Platibus.UnitTests.SQLite
{
    public class SQLiteMessageQueueingServiceTests : MessageQueueingServiceTests<SQLiteMessageQueueingService>
    {
        private readonly DirectoryInfo _baseDirectory;

        public SQLiteMessageQueueingServiceTests() 
            : this(SQLiteCollectionFixture.Instance)
        {
        }

        public SQLiteMessageQueueingServiceTests(SQLiteCollectionFixture fixture)
            : base(fixture.MessageQueueingService)
        {
            _baseDirectory = fixture.BaseDirectory;
        }

        protected override async Task GivenExistingQueuedMessage(QueueName queueName, Message message, IPrincipal principal)
        {
            using (var queueInspector = new SQLiteMessageQueueInspector(_baseDirectory, queueName))
            {
                await queueInspector.InsertMessage(message, principal);
            }
        }

        protected override async Task<bool> MessageQueued(QueueName queueName, Message message)
        {
            var messageId = message.Headers.MessageId;
            using (var queueInspector = new SQLiteMessageQueueInspector(_baseDirectory, queueName))
            {
                var messagesInQueue = await queueInspector.EnumerateMessages();
                return messagesInQueue.Any(m => m.Message.Headers.MessageId == messageId);
            }
        }

        protected override async Task<bool> MessageDead(QueueName queueName, Message message)
        {
            var messageId = message.Headers.MessageId;
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddSeconds(-5);
            using (var queueInspector = new SQLiteMessageQueueInspector(_baseDirectory, queueName))
            {
                var messagesInQueue = await queueInspector.EnumerateAbandonedMessages(startDate, endDate);
                return messagesInQueue.Any(m => m.Message.Headers.MessageId == messageId);
            }
        }
    }
}