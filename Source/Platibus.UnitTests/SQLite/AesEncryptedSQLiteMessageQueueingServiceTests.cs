using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Platibus.Queueing;
using Platibus.SQLite;
using Xunit;

namespace Platibus.UnitTests.SQLite
{
    [Trait("Category", "UnitTests")]
    [Trait("Dependency", "SQLite")]
    [Collection(SQLiteCollection.Name)]
    public class AesEncryptedSQLiteMessageQueueingServiceTests : MessageQueueingServiceTests<SQLiteMessageQueueingService>
    {
        private readonly DirectoryInfo _queueDirectory;
        
        public AesEncryptedSQLiteMessageQueueingServiceTests(SQLiteFixture fixture)
            : base(fixture.DiagnosticService, fixture.MessageQueueingService)
        {
            _queueDirectory = fixture.QueueDirectory;
        }

        protected override async Task GivenExistingQueuedMessage(QueueName queueName, Message message, IPrincipal principal)
        {
            using (var queueInspector = new SQLiteMessageQueueInspector(_queueDirectory, queueName, SecurityTokenService, null))
            {
                await queueInspector.Init();
                await queueInspector.InsertMessage(new QueuedMessage(message, principal));
            }
        }

        protected override async Task<bool> MessageQueued(QueueName queueName, Message message)
        {
            var messageId = message.Headers.MessageId;
            using (var queueInspector = new SQLiteMessageQueueInspector(_queueDirectory, queueName, SecurityTokenService, null))
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
            using (var queueInspector = new SQLiteMessageQueueInspector(_queueDirectory, queueName, SecurityTokenService, null))
            {
                await queueInspector.Init();
                var messagesInQueue = await queueInspector.EnumerateAbandonedMessages(startDate, endDate);
                return messagesInQueue.Any(m => m.Message.Headers.MessageId == messageId);
            }
        }
    }
}