using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Platibus.RabbitMQ;
using RabbitMQ.Client;

namespace Platibus.UnitTests
{
    internal class RabbitMQMessageQueueingServiceTests
    {
        public static readonly Uri RabbitMQUri = new Uri("amqp://localhost:5672");

        [Test]
        public async Task Given_Existing_Queue_When_New_Message_Queued_Then_Listener_Should_Fire()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            rmqQueueingService.Init();

            try
            {
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


                await rmqQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions {MaxAttempts = 1});

                var message = new Message(new MessageHeaders
                {
                    {HeaderName.ContentType, "text/plain"},
                    {HeaderName.MessageId, Guid.NewGuid().ToString()}
                }, "Hello, world!");

                await rmqQueueingService.EnqueueMessage(queueName, message, Thread.CurrentPrincipal);
                await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(1));

                var messageEqualityComparer = new MessageEqualityComparer();
                mockListener.Verify(
                    x =>
                        x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                            It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

                Assert.That(GetQueueDepth(queueName), Is.EqualTo(0));
                Assert.That(GetQueueDepth(queueName.GetRetryQueueName()), Is.EqualTo(0));
            }
            finally
            {
                rmqQueueingService.DeleteQueue(queueName);
            }
        }

        [Test]
        public async Task Given_Queued_Message_When_Acknowledged_Then_Message_Should_Be_Deleted()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            rmqQueueingService.Init();
            try
            {
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

                await rmqQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions {MaxAttempts = 1});

                var message = new Message(new MessageHeaders
                {
                    {HeaderName.ContentType, "text/plain"},
                    {HeaderName.MessageId, Guid.NewGuid().ToString()}
                }, "Hello, world!");

                await rmqQueueingService.EnqueueMessage(queueName, message, Thread.CurrentPrincipal);
                await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(1));

                // The listener is called before the message is published to the retry queue, 
                // so there is a possible race condition here.  Wait for a second to allow the 
                // publish to take place before checking the queueu and retry queue depth.
                await Task.Delay(TimeSpan.FromSeconds(1));

                var messageEqualityComparer = new MessageEqualityComparer();
                mockListener.Verify(
                    x =>
                        x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                            It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

                Assert.That(GetQueueDepth(queueName), Is.EqualTo(0));
                Assert.That(GetQueueDepth(queueName.GetRetryQueueName()), Is.EqualTo(0));
            }
            finally
            {
                rmqQueueingService.DeleteQueue(queueName);
            }
        }

        [Test]
        public async Task Given_Queued_Message_When_Not_Acknowledged_Then_Message_Published_To_Retry_Queue()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            rmqQueueingService.Init();

            try
            {
                var mockListener = new Mock<IQueueListener>();
                mockListener.Setup(
                    x =>
                        x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(),
                            It.IsAny<CancellationToken>()))
                    .Callback<Message, IQueuedMessageContext, CancellationToken>(
                        (msg, ctx, ct) => { listenerCalledEvent.Set(); })
                    .Returns(Task.FromResult(true));

                await rmqQueueingService
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

                await rmqQueueingService.EnqueueMessage(queueName, message, Thread.CurrentPrincipal);
                await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(3));

                // The listener is called before the message is published to the retry queue, 
                // so there is a possible race condition here.  Wait for a second to allow the 
                // publish to take place before checking the retry queue depth.
                await Task.Delay(TimeSpan.FromSeconds(1));

                var messageEqualityComparer = new MessageEqualityComparer();
                mockListener.Verify(
                    x =>
                        x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                            It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

                Assert.That(GetQueueDepth(queueName), Is.EqualTo(0));
                Assert.That(GetQueueDepth(queueName.GetRetryQueueName()), Is.EqualTo(1));
            }
            finally
            {
                rmqQueueingService.DeleteQueue(queueName);
            }
        }

        [Test]
        public async Task Given_Auto_Acknowledge_Queue_When_Not_Acknowledged_Then_Message_Should_Be_Deleted()
        {
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            rmqQueueingService.Init();

            try
            {
                var listenerCalledEvent = new ManualResetEvent(false);
                var mockListener = new Mock<IQueueListener>();
                mockListener.Setup(x =>
                    x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(),
                        It.IsAny<CancellationToken>()))
                    .Callback<Message, IQueuedMessageContext, CancellationToken>(
                        (msg, ctx, ct) => { listenerCalledEvent.Set(); })
                    .Returns(Task.FromResult(true));

                await rmqQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions {AutoAcknowledge = true});

                var message = new Message(new MessageHeaders
                {
                    {HeaderName.ContentType, "text/plain"},
                    {HeaderName.MessageId, Guid.NewGuid().ToString()}
                }, "Hello, world!");

                await rmqQueueingService.EnqueueMessage(queueName, message, Thread.CurrentPrincipal);
                await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(1));

                // The listener is called before the file is deleted, so there is a possible
                // race condition here.  Wait for a second to allow the delete to take place
                // before enumerating the files to see that they were actually deleted.
                await Task.Delay(TimeSpan.FromSeconds(1));

                var messageEqualityComparer = new MessageEqualityComparer();
                mockListener.Verify(x =>
                    x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                        It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

                Assert.That(GetQueueDepth(queueName), Is.EqualTo(0));
                Assert.That(GetQueueDepth(queueName.GetRetryQueueName()), Is.EqualTo(0));
            }
            finally
            {
                rmqQueueingService.DeleteQueue(queueName);
            }
        }

        private static uint GetQueueDepth(QueueName queueName)
        {
            var connectionFactory = new ConnectionFactory {Uri = RabbitMQUri.ToString()};
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var result = channel.QueueDeclarePassive(queueName);
                return result.MessageCount;
            }
        }

        private static async Task StageExistingMessage(Message message, QueueName queueName)
        {
            var connectionFactory = new ConnectionFactory {Uri = RabbitMQUri.ToString()};
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // We have to declare the queue as a persistence queue because this is 
                // called before the queue is created by the RabbitMQQueueingService
                channel.QueueDeclare(queueName, true, false, false, null);
                await RabbitMQHelper.PublishMessage(message, Thread.CurrentPrincipal, channel, queueName);
            }
        }

        [Test]
        public async Task Given_Existing_Message_When_Creating_Queue_Then_Listener_Should_Fire()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            rmqQueueingService.Init();

            try
            {
                var message = new Message(new MessageHeaders
                {
                    {HeaderName.ContentType, "text/plain"},
                    {HeaderName.MessageId, Guid.NewGuid().ToString()}
                }, "Hello, world!");

                await StageExistingMessage(message, queueName);

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

                await rmqQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions {MaxAttempts = 1, RetryDelay = TimeSpan.FromSeconds(30)});
                await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(5));

                var messageEqualityComparer = new MessageEqualityComparer();
                mockListener.Verify(
                    x =>
                        x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                            It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

                Assert.That(GetQueueDepth(queueName), Is.EqualTo(0));
                Assert.That(GetQueueDepth(queueName.GetRetryQueueName()), Is.EqualTo(0));
            }
            finally
            {
                rmqQueueingService.DeleteQueue(queueName);
            }
        }

        [Test]
        public async Task Given_10_Existing_Messages_10_New_Messages_Then_Listener_Should_Fire_For_All_20_Messages()
        {
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            rmqQueueingService.Init();

            try
            {
                var existingMessages = Enumerable.Range(1, 10)
                    .Select(i => new Message(new MessageHeaders
                    {
                        {HeaderName.ContentType, "text/plain"},
                        {HeaderName.MessageId, Guid.NewGuid().ToString()}
                    }, "Hello, world! (" + i + ")"))
                    .ToList();

                await Task.WhenAll(existingMessages.Select(msg => StageExistingMessage(msg, queueName)));

                var newMessages = Enumerable.Range(11, 20)
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

                await rmqQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions {MaxAttempts = 1});

                await Task.WhenAll(newMessages.Select(msg =>
                    rmqQueueingService.EnqueueMessage(queueName, msg, Thread.CurrentPrincipal)));

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

                Assert.That(GetQueueDepth(queueName), Is.EqualTo(0));
                Assert.That(GetQueueDepth(queueName.GetRetryQueueName()), Is.EqualTo(0));
            }
            finally
            {
                rmqQueueingService.DeleteQueue(queueName);
            }
        }
    }
}