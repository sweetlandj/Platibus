using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Platibus.SQL;

namespace Platibus.UnitTests.LocalDB
{
    public class LocalDBMessageQueueingServiceTests : MessageQueueingServiceTests<SQLMessageQueueingService>
    {
        public LocalDBMessageQueueingServiceTests() 
            : this(LocalDBCollectionFixture.Instance)
        {
        }

        public LocalDBMessageQueueingServiceTests(LocalDBCollectionFixture fixture)
            : base(fixture.MessageQueueingService)
        {
        }

        protected override async Task GivenExistingQueuedMessage(QueueName queueName, Message message, IPrincipal principal)
        {
            using (var queueInspector = new SQLMessageQueueInspector(MessageQueueingService, queueName, SecurityTokenService))
            {
                await queueInspector.InsertMessage(message, principal);
            }
        }

        protected override async Task<bool> MessageQueued(QueueName queueName, Message message)
        {
            var messageId = message.Headers.MessageId;
            using (var queueInspector = new SQLMessageQueueInspector(MessageQueueingService, queueName, SecurityTokenService))
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
            using (var queueInspector = new SQLMessageQueueInspector(MessageQueueingService, queueName, SecurityTokenService))
            {
                var messagesInQueue = await queueInspector.EnumerateAbandonedMessages(startDate, endDate);
                return messagesInQueue.Any(m => m.Message.Headers.MessageId == messageId);
            }
        }
    }
}
