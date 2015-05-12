using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Platibus.RabbitMQ;
using RabbitMQ.Client;

namespace Platibus.UnitTests
{
    class RabbitMQMessageQueueingServiceTests
    {
        public static readonly Uri RabbitMQUri = new Uri("amqp://localhost:5672");
        
        [Test]
        public async Task Given_Existing_Queue_When_New_Message_Queued_Then_Listener_Should_Fire()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            rmqQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x => x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    ctx.Acknowledge();
                    listenerCalledEvent.Set();
                })
                .Returns(Task.FromResult(true));

            var queueName = new QueueName(Guid.NewGuid().ToString());
            await rmqQueueingService
                .CreateQueue(queueName, mockListener.Object, new QueueOptions { MaxAttempts = 1 })
                .ConfigureAwait(false);

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            await rmqQueueingService
                .EnqueueMessage(queueName, message, Thread.CurrentPrincipal)
                .ConfigureAwait(false);

            await listenerCalledEvent
                .WaitOneAsync(TimeSpan.FromSeconds(1))
                .ConfigureAwait(false);

            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(x => x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());            
        }

        [Test]
        public async Task Given_Queued_Message_When_Acknowledged_Then_Message_Should_Be_Deleted()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var queueName = new QueueName(Guid.NewGuid().ToString());
            
            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            rmqQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x => x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    ctx.Acknowledge();
                    listenerCalledEvent.Set();
                })
                .Returns(Task.FromResult(true));

            await rmqQueueingService
                .CreateQueue(queueName, mockListener.Object, new QueueOptions { MaxAttempts = 1 })
                .ConfigureAwait(false);

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            await rmqQueueingService
                .EnqueueMessage(queueName, message, Thread.CurrentPrincipal)
                .ConfigureAwait(false);

            await listenerCalledEvent
                .WaitOneAsync(TimeSpan.FromSeconds(1))
                .ConfigureAwait(false);

            // The listener is called before the file is deleted, so there is a possible
            // race condition here.  Wait for a second to allow the delete to take place
            // before enumerating the files to see that they were actually deleted.
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(x => x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

            Assert.That(GetQueueDepth(queueName), Is.EqualTo(0));
        }

        [Test]
        public async Task Given_Queued_Message_When_Not_Acknowledged_Then_Message_Should_Not_Be_Deleted()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            rmqQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x => x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    listenerCalledEvent.Set();
                })
                .Returns(Task.FromResult(true));

            await rmqQueueingService
                .CreateQueue(queueName, mockListener.Object, new QueueOptions 
                { 
                    MaxAttempts = 2, // Prevent message from being sent to the DLQ,
                    RetryDelay = TimeSpan.FromSeconds(30)
                })
                .ConfigureAwait(false);

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            await rmqQueueingService
                .EnqueueMessage(queueName, message, Thread.CurrentPrincipal)
                .ConfigureAwait(false);

            await listenerCalledEvent
                .WaitOneAsync(TimeSpan.FromSeconds(1))
                .ConfigureAwait(false);

            // The listener is called before the file is deleted, so there is a possible
            // race condition here.  Wait for a second to allow the delete to take place
            // before enumerating the files to see that they were actually not deleted.
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);


            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(x => x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

            Assert.That(GetQueueDepth(queueName), Is.EqualTo(1));
        }

        [Test]
        public async Task Given_Auto_Acknowledge_Queue_When_Not_Acknowledged_Then_Message_Should_Be_Deleted()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            rmqQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x => x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    listenerCalledEvent.Set();
                })
                .Returns(Task.FromResult(true));

            await rmqQueueingService
                .CreateQueue(queueName, mockListener.Object, new QueueOptions { AutoAcknowledge = true })
                .ConfigureAwait(false);

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            await rmqQueueingService
                .EnqueueMessage(queueName, message, Thread.CurrentPrincipal)
                .ConfigureAwait(false);

            await listenerCalledEvent
                .WaitOneAsync(TimeSpan.FromSeconds(1))
                .ConfigureAwait(false);

            // The listener is called before the file is deleted, so there is a possible
            // race condition here.  Wait for a second to allow the delete to take place
            // before enumerating the files to see that they were actually deleted.
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(x => x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

            Assert.That(GetQueueDepth(queueName), Is.EqualTo(0));
        }

        [Test]
        public async Task Given_Auto_Acknowledge_Queue_When_Listener_Throws_Then_Message_Should_Not_Be_Deleted()
        {
            var listenerCalledEvent = new ManualResetEvent(false);            
            var queueName = new QueueName(Guid.NewGuid().ToString());

            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            rmqQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x => x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    listenerCalledEvent.Set();
                    throw new Exception();
                });

            await rmqQueueingService
                .CreateQueue(queueName, mockListener.Object, new QueueOptions 
                { 
                    AutoAcknowledge = true, 
                    MaxAttempts = 2, // So the message doesn't get moved to the DLQ
                    RetryDelay = TimeSpan.FromSeconds(30) 
                })
                .ConfigureAwait(false);

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            await rmqQueueingService
                .EnqueueMessage(queueName, message, Thread.CurrentPrincipal)
                .ConfigureAwait(false);

            var listenerCalled = await listenerCalledEvent
                .WaitOneAsync(Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromSeconds(3))
                .ConfigureAwait(false);

            Assert.That(listenerCalled, Is.True);

            // The listener is called before the file is deleted, so there is a possible
            // race condition here.  Wait for a second to allow the delete to take place
            // before enumerating the files to see that they were actually not deleted.
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(x => x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

            Assert.That(GetQueueDepth(queueName), Is.EqualTo(1));

            //var queuedMessages = queueDir.EnumerateFiles()
            //    .Select(f => new MessageFile(f))
            //    .ToList();

            //Assert.That(queuedMessages.Count, Is.EqualTo(1));
            //Assert.That(await queuedMessages[0].ReadMessage(), Is.EqualTo(message).Using(messageEqualityComparer));
        }

        private static uint GetQueueDepth(QueueName queueName)
        {
            var connectionFactory = new ConnectionFactory { Uri = RabbitMQUri.ToString() };
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var result = channel.QueueDeclarePassive(queueName);
                return result.MessageCount;
            }   
        }

        private static async Task StageExistingMessage(Message message, QueueName queueName)
        {
            var connectionFactory = new ConnectionFactory { Uri = RabbitMQUri.ToString() };
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queueName, true, false, false, null);
                using (var stringWriter = new StringWriter())
                using (var messageWriter = new MessageWriter(stringWriter))
                {
                    await messageWriter.WritePrincipal(Thread.CurrentPrincipal);
                    await messageWriter.WriteMessage(message);
                    channel.BasicPublish("", queueName, null, Encoding.UTF8.GetBytes(stringWriter.ToString()));
                }
            }
        }

        [Test]
        public async Task Given_Existing_Message_When_Creating_Queue_Then_Listener_Should_Fire()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var queueName = new QueueName(Guid.NewGuid().ToString());

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            await StageExistingMessage(message, queueName);
            
            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            rmqQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x => x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    ctx.Acknowledge();
                    listenerCalledEvent.Set();
                })
                .Returns(Task.FromResult(true));

            await rmqQueueingService
                .CreateQueue(queueName, mockListener.Object, new QueueOptions { MaxAttempts = 1 })
                .ConfigureAwait(false);

            await listenerCalledEvent
                .WaitOneAsync(TimeSpan.FromSeconds(1))
                .ConfigureAwait(false);

            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(x => x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());
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

            var queueName = new QueueName(Guid.NewGuid().ToString());

            await Task.WhenAll(existingMessages
                .Select(async msg => await StageExistingMessage(msg, queueName)))
                .ConfigureAwait(false);

            var newMessages = Enumerable.Range(1, 10)
                .Select(i => new Message(new MessageHeaders
                {
                    {HeaderName.ContentType, "text/plain"},
                    {HeaderName.MessageId, Guid.NewGuid().ToString()}
                }, "Hello, world! (" + i + ")"))
                .ToList();

            var listenerCountdown = new CountdownEvent(existingMessages.Count + newMessages.Count);
            
            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            rmqQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x => x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    ctx.Acknowledge();
                    listenerCountdown.Signal();
                })
                .Returns(Task.FromResult(true));

            await rmqQueueingService
                .CreateQueue(queueName, mockListener.Object, new QueueOptions { MaxAttempts = 1 })
                .ConfigureAwait(false);

            await Task.WhenAll(newMessages.Select(msg => rmqQueueingService.EnqueueMessage(queueName, msg, Thread.CurrentPrincipal)))
                .ConfigureAwait(false);

            var timedOut = !await listenerCountdown.WaitHandle.WaitOneAsync(TimeSpan.FromSeconds(10));

            Assert.That(timedOut, Is.False, "Timed out waiting for listeners to be called");

            var messageEqualityComparer = new MessageEqualityComparer();
            var allmessages = existingMessages.Union(newMessages);
            foreach(var message in allmessages)
            {
                mockListener.Verify(x => x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());
            }
        }
    }
}