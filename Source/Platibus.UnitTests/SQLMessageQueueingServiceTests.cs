using Moq;
using NUnit.Framework;
using Platibus.SQL;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.UnitTests
{
    class SQLMessageQueueingServiceTests
    {
        protected ConnectionStringSettings GetConnectionStringSettings()
        {
            var connectionStrings = ConfigurationManager.ConnectionStrings["PlatibusUnitTests"];
            using (var connection = connectionStrings.OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    IF (OBJECT_ID('[PB_QueuedMessages]')) IS NOT NULL 
                    BEGIN
                        DELETE FROM [PB_QueuedMessages]
                    END

                    IF (OBJECT_ID('[PB_MessageQueues]')) IS NOT NULL
                    BEGIN
                        DELETE FROM [PB_MessageQueues]
                    END";

                command.ExecuteNonQuery();
            }
            return connectionStrings;
        }

        [Test]
        public async Task Given_Existing_Queue_When_New_Message_Queued_Then_Listener_Should_Fire()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var connectionStringSettings = GetConnectionStringSettings();
            var sqlQueueingService = new SQLMessageQueueingService(connectionStringSettings);
            sqlQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x => x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    ctx.Acknowledge();
                    listenerCalledEvent.Set();
                })
                .Returns(Task.FromResult(true));

            var queueName = new QueueName(Guid.NewGuid().ToString());
            await sqlQueueingService
                .CreateQueue(queueName, mockListener.Object, new QueueOptions { MaxAttempts = 1 })
                .ConfigureAwait(false);

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            await sqlQueueingService
                .EnqueueMessage(queueName, message, Thread.CurrentPrincipal)
                .ConfigureAwait(false);

            await listenerCalledEvent
                .WaitOneAsync(TimeSpan.FromSeconds(1))
                .ConfigureAwait(false);

            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(x => x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());            
        }
    }
}
