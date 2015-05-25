using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Platibus.SQL;

namespace Platibus.UnitTests
{
    internal class SQLMessageQueueingServiceTests
    {
        protected ConnectionStringSettings GetConnectionStringSettings()
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["PlatibusUnitTests.LocalDB"];
            using (var connection = connectionStringSettings.OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    IF (OBJECT_ID('[PB_QueuedMessages]')) IS NOT NULL 
                    BEGIN
                        DELETE FROM [PB_QueuedMessages]
                    END";

                command.ExecuteNonQuery();
            }
            return connectionStringSettings;
        }

        [Test]
        public async Task When_New_Message_Queued_Then_Listener_Should_Fire()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var connectionStringSettings = GetConnectionStringSettings();
            var sqlQueueingService = new SQLMessageQueueingService(connectionStringSettings);
            sqlQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(
                x =>
                    x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(),
                        It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    ctx.Acknowledge();
                    listenerCalledEvent.Set();
                })
                .Returns(Task.FromResult(true));

            var queueName = new QueueName(Guid.NewGuid().ToString());
            await sqlQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions {MaxAttempts = 1});

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            await sqlQueueingService.EnqueueMessage(queueName, message, Thread.CurrentPrincipal);
            await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(1));

            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(
                x =>
                    x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                        It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Test]
        public async Task Given_Queued_Message_When_Context_Acknowledged_Then_Message_Should_Be_Acknowledged()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var connectionStringSettings = GetConnectionStringSettings();
            var queueName = new QueueName(Guid.NewGuid().ToString());

            var sqlQueueingService = new SQLMessageQueueingService(connectionStringSettings);
            sqlQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(
                x =>
                    x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(),
                        It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    ctx.Acknowledge();
                    listenerCalledEvent.Set();
                })
                .Returns(Task.FromResult(true));

            await sqlQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions {MaxAttempts = 1});

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            await sqlQueueingService.EnqueueMessage(queueName, message, Thread.CurrentPrincipal);
            await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(1));

            // The listener is called before the row is updated, so there is a possible
            // race condition here.  Wait for a second to allow the update to take place
            // before enumerating the rows to see that they were actually deleted.
            await Task.Delay(TimeSpan.FromSeconds(1));

            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(
                x =>
                    x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                        It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

            var sqlQueueInspector = new SQLMessageQueueInspector(sqlQueueingService, queueName);
            var queuedMessages = (await sqlQueueInspector.EnumerateMessages()).ToList();

            Assert.That(queuedMessages, Is.Empty);
        }

        [Test]
        public async Task Given_Queued_Message_When_Context_Not_Acknowledged_Then_Message_Should_Not_Be_Acknowledged()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var connectionStringSettings = GetConnectionStringSettings();
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var sqlQueueingService = new SQLMessageQueueingService(connectionStringSettings);
            sqlQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(
                x =>
                    x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(),
                        It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>(
                    (msg, ctx, ct) => { listenerCalledEvent.Set(); })
                .Returns(Task.FromResult(true));

            await sqlQueueingService
                .CreateQueue(queueName, mockListener.Object, new QueueOptions
                {
                    MaxAttempts = 2, // Prevent message from being sent to the DLQ,
                    RetryDelay = TimeSpan.FromSeconds(30)
                });

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            await sqlQueueingService.EnqueueMessage(queueName, message, Thread.CurrentPrincipal);
            await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(1));

            // The listener is called before the row is updated, so there is a possible
            // race condition here.  Wait for a second to allow the update to take place
            // before enumerating the rows to see that they were actually not updated.
            await Task.Delay(TimeSpan.FromSeconds(1));

            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(
                x =>
                    x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                        It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

            var sqlQueueInspector = new SQLMessageQueueInspector(sqlQueueingService, queueName);
            var queuedMessages = (await sqlQueueInspector.EnumerateMessages()).ToList();

            Assert.That(queuedMessages.Count, Is.EqualTo(1));
            Assert.That(queuedMessages[0].Message, Is.EqualTo(message).Using(messageEqualityComparer));
        }

        [Test]
        public async Task Given_Auto_Acknowledge_Queue_When_Context_Not_Acknowledged_Then_Message_Should_Be_Acknowledged
            ()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var connectionStringSettings = GetConnectionStringSettings();
            var queueName = new QueueName(Guid.NewGuid().ToString());

            var sqlQueueingService = new SQLMessageQueueingService(connectionStringSettings);
            sqlQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(
                x =>
                    x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(),
                        It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>(
                    (msg, ctx, ct) => { listenerCalledEvent.Set(); })
                .Returns(Task.FromResult(true));

            await sqlQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions {AutoAcknowledge = true});

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            await sqlQueueingService.EnqueueMessage(queueName, message, Thread.CurrentPrincipal);
            await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(1));

            // The listener is called before the row is updated, so there is a possible
            // race condition here.  Wait for a second to allow the update to take place
            // before enumerating the rows to see that they were actually deleted.
            await Task.Delay(TimeSpan.FromSeconds(1));

            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(x =>
                x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                    It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

            var sqlQueueInspector = new SQLMessageQueueInspector(sqlQueueingService, queueName);
            var queuedMessages = (await sqlQueueInspector.EnumerateMessages()).ToList();

            Assert.That(queuedMessages, Is.Empty);
        }

        [Test]
        public async Task Given_Auto_Acknowledge_Queue_When_Listener_Throws_Then_Message_Should_Not_Be_Acknowledged()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var connectionStringSettings = GetConnectionStringSettings();
            var queueName = new QueueName(Guid.NewGuid().ToString());

            var sqlQueueingService = new SQLMessageQueueingService(connectionStringSettings);
            sqlQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x =>
                x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    listenerCalledEvent.Set();
                    throw new Exception();
                });

            await sqlQueueingService
                .CreateQueue(queueName, mockListener.Object, new QueueOptions
                {
                    AutoAcknowledge = true,
                    MaxAttempts = 2, // So the message doesn't get moved to the DLQ
                    RetryDelay = TimeSpan.FromSeconds(30)
                });

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            await sqlQueueingService.EnqueueMessage(queueName, message, Thread.CurrentPrincipal);

            var listenerCalled = await listenerCalledEvent
                .WaitOneAsync(Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromSeconds(3));

            Assert.That(listenerCalled, Is.True);

            // The listener is called before the row is updated, so there is a possible
            // race condition here.  Wait for a second to allow the update to take place
            // before enumerating the rows to see that they were actually not updated.
            await Task.Delay(TimeSpan.FromSeconds(1));

            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(x =>
                x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                    It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

            var sqlQueueInspector = new SQLMessageQueueInspector(sqlQueueingService, queueName);
            var queuedMessages = (await sqlQueueInspector.EnumerateMessages()).ToList();

            Assert.That(queuedMessages[0].Message, Is.EqualTo(message).Using(messageEqualityComparer));
        }

        [Test]
        public async Task Given_Existing_Message_When_Creating_Queue_Then_Listener_Should_Fire()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var connectionStringSettings = GetConnectionStringSettings();
            var queueName = new QueueName(Guid.NewGuid().ToString());

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            var sqlQueueingService = new SQLMessageQueueingService(connectionStringSettings);
            sqlQueueingService.Init();

            // Insert a test message before creating queue
            var sqlQueueInspector = new SQLMessageQueueInspector(sqlQueueingService, queueName);
            await sqlQueueInspector.InsertMessage(message, Thread.CurrentPrincipal);

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(
                x =>
                    x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(),
                        It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    ctx.Acknowledge();
                    listenerCalledEvent.Set();
                })
                .Returns(Task.FromResult(true));

            // Create the queue, which should trigger the inserted message to be enqueued
            // and processed
            await sqlQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions {MaxAttempts = 1});
            await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(1));

            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(
                x =>
                    x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                        It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Test]
        public async Task Given_10_Existing_Messages_10_New_Messages_Then_Listener_Should_Fire_For_All_20_Messages()
        {
            var existingMessages = Enumerable.Range(1, 10)
                .Select(i => new Message(new MessageHeaders
                {
                    {HeaderName.ContentType, "text/plain"},
                    {HeaderName.MessageId, Guid.NewGuid().ToString()}
                }, "Hello, world! (" + i + ")"))
                .ToList();

            var connectionStringSettings = GetConnectionStringSettings();
            var queueName = new QueueName(Guid.NewGuid().ToString());

            var sqlQueueingService = new SQLMessageQueueingService(connectionStringSettings);
            sqlQueueingService.Init();

            var sqlQueueInspector = new SQLMessageQueueInspector(sqlQueueingService, queueName);
            foreach (var msg in existingMessages)
            {
                await sqlQueueInspector.InsertMessage(msg, Thread.CurrentPrincipal);
            }

            var newMessages = Enumerable.Range(1, 10)
                .Select(i => new Message(new MessageHeaders
                {
                    {HeaderName.ContentType, "text/plain"},
                    {HeaderName.MessageId, Guid.NewGuid().ToString()}
                }, "Hello, world! (" + i + ")"))
                .ToList();

            var listenerCountdown = new CountdownEvent(existingMessages.Count + newMessages.Count);

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(
                x =>
                    x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(),
                        It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    ctx.Acknowledge();
                    listenerCountdown.Signal();
                })
                .Returns(Task.FromResult(true));

            await sqlQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions {MaxAttempts = 1});
            await Task.WhenAll(newMessages.Select(msg => sqlQueueingService.EnqueueMessage(queueName, msg, Thread.CurrentPrincipal)));

            var timedOut = !await listenerCountdown.WaitHandle.WaitOneAsync(TimeSpan.FromSeconds(10));

            Assert.That(timedOut, Is.False, "Timed out waiting for listeners to be called");

            var messageEqualityComparer = new MessageEqualityComparer();
            var allmessages = existingMessages.Union(newMessages);
            foreach (var message in allmessages)
            {
                mockListener.Verify(
                    x =>
                        x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                            It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());
            }
        }
    }
}