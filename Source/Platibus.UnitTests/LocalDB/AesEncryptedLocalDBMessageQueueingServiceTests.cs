using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Platibus.Queueing;
using Platibus.SQL;
using Xunit;

namespace Platibus.UnitTests.LocalDB
{
    [Trait("Category", "UnitTests")]
    [Trait("Dependency", "LocalDB")]
    [Collection(LocalDBCollection.Name)]
    public class AesEncryptedLocalDBMessageQueueingServiceTests : MessageQueueingServiceTests<SQLMessageQueueingService>
    {
        public AesEncryptedLocalDBMessageQueueingServiceTests(LocalDBFixture fixture)
            : base(fixture.MessageQueueingService)
        {
        }

        protected override async Task GivenExistingQueuedMessage(QueueName queueName, Message message, IPrincipal principal)
        {
            using (var queueInspector = new SQLMessageQueueInspector(MessageQueueingService, queueName, SecurityTokenService, null))
            {
                await queueInspector.Init();
                await queueInspector.InsertMessage(new QueuedMessage(message, principal));
            }
        }

        protected override async Task<bool> MessageQueued(QueueName queueName, Message message)
        {
            var messageId = message.Headers.MessageId;
            using (var queueInspector = new SQLMessageQueueInspector(MessageQueueingService, queueName, SecurityTokenService, null))
            {
                await queueInspector.Init();
                var messagesInQueue = await queueInspector.EnumerateMessages();
                return messagesInQueue.Any(m => m.Message.Headers.MessageId == messageId);
            }
        }

        protected override async Task<bool> MessageDead(QueueName queueName, Message message)
        {
            var messageId = message.Headers.MessageId;
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddSeconds(-5);
            using (var queueInspector = new SQLMessageQueueInspector(MessageQueueingService, queueName, SecurityTokenService, null))
            {
                await queueInspector.Init();
                var messagesInQueue = await queueInspector.EnumerateAbandonedMessages(startDate, endDate);
                return messagesInQueue.Any(m => m.Message.Headers.MessageId == messageId);
            }
        }
    }
}