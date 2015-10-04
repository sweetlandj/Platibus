using System;
using System.Collections.Generic;
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
        public static readonly Uri RabbitMQUri = new Uri("amqp://test:test@localhost:5672/test");

        [Test]
        public async Task Given_Existing_Queue_When_New_Message_Queued_Then_Listener_Should_Fire()
        {
            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            var listenerCalledEvent = new ManualResetEvent(false);
            var queueName = new QueueName(Guid.NewGuid().ToString());

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x =>
                x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    ctx.Acknowledge();
                    listenerCalledEvent.Set();
                })
                .Returns(Task.FromResult(true));

            var cts = new CancellationTokenSource();
            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            try
            {
                var ct = cts.Token;
                await rmqQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions {MaxAttempts = 1}, ct);
                await rmqQueueingService.EnqueueMessage(queueName, message, Thread.CurrentPrincipal, ct);
                await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(1));

                var messageEqualityComparer = new MessageEqualityComparer();
                mockListener.Verify(x =>
                    x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                        It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

                Assert.That(GetQueueDepth(queueName), Is.EqualTo(0));
                Assert.That(GetQueueDepth(queueName.GetRetryQueueName()), Is.EqualTo(0));
            }
            finally
            {
                rmqQueueingService.Dispose();
                cts.Dispose();
                DeleteQueue(queueName);
            }
        }

        [Test]
        public async Task Given_Queued_Message_When_Acknowledged_Then_Message_Should_Be_Deleted()
        {
            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            var listenerCalledEvent = new ManualResetEvent(false);
            var queueName = new QueueName(Guid.NewGuid().ToString());

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x =>
                x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    ctx.Acknowledge();
                    listenerCalledEvent.Set();
                })
                .Returns(Task.FromResult(true));

            var cts = new CancellationTokenSource();
            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            try
            {
                var ct = cts.Token;
                await rmqQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions {MaxAttempts = 1}, ct);
                await rmqQueueingService.EnqueueMessage(queueName, message, Thread.CurrentPrincipal, ct);
                await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(1));

                // The listener is called before the message is published to the retry queue, 
                // so there is a possible race condition here.  Wait for a second to allow the 
                // publish to take place before checking the queueu and retry queue depth.
                await Task.Delay(TimeSpan.FromSeconds(1), ct);

                var messageEqualityComparer = new MessageEqualityComparer();
                mockListener.Verify(x =>
                    x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                        It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

                Assert.That(GetQueueDepth(queueName), Is.EqualTo(0));
                Assert.That(GetQueueDepth(queueName.GetRetryQueueName()), Is.EqualTo(0));
            }
            finally
            {
                rmqQueueingService.Dispose();
                cts.Dispose();
                DeleteQueue(queueName);
            }
        }

        [Test]
        public async Task Given_Queued_Message_When_Not_Acknowledged_Then_Message_Published_To_Retry_Queue()
        {
            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            var listenerCalledEvent = new ManualResetEvent(false);
            var queueName = new QueueName(Guid.NewGuid().ToString());

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x =>
                x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>(
                    (msg, ctx, ct) => { listenerCalledEvent.Set(); })
                .Returns(Task.FromResult(true));

            var cts = new CancellationTokenSource();
            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            try
            {
                var ct = cts.Token;
                await rmqQueueingService
                    .CreateQueue(queueName, mockListener.Object, new QueueOptions
                    {
                        MaxAttempts = 2, // Prevent message from being sent to the DLQ,
                        RetryDelay = TimeSpan.FromSeconds(30)
                    }, ct);

                await rmqQueueingService.EnqueueMessage(queueName, message, Thread.CurrentPrincipal, ct);
                await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(3));

                // The listener is called before the message is published to the retry queue, 
                // so there is a possible race condition here.  Wait for a second to allow the 
                // publish to take place before checking the retry queue depth.
                await Task.Delay(TimeSpan.FromSeconds(1), ct);

                var messageEqualityComparer = new MessageEqualityComparer();
                mockListener.Verify(x =>
                    x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                        It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

                Assert.That(GetQueueDepth(queueName), Is.EqualTo(0));
                Assert.That(GetQueueDepth(queueName.GetRetryQueueName()), Is.EqualTo(1));
            }
            finally
            {
                rmqQueueingService.Dispose();
                cts.Dispose();
                DeleteQueue(queueName);
            }
        }

        [Test]
        public async Task Given_Auto_Acknowledge_Queue_When_Not_Acknowledged_Then_Message_Should_Be_Deleted()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var queueName = new QueueName(Guid.NewGuid().ToString());

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x =>
                x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>(
                    (msg, ctx, ct) => { listenerCalledEvent.Set(); })
                .Returns(Task.FromResult(true));

            var cts = new CancellationTokenSource();
            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            try
            {
                var ct = cts.Token;
                await rmqQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions {AutoAcknowledge = true}, ct);

                var message = new Message(new MessageHeaders
                {
                    {HeaderName.ContentType, "text/plain"},
                    {HeaderName.MessageId, Guid.NewGuid().ToString()}
                }, "Hello, world!");

                await rmqQueueingService.EnqueueMessage(queueName, message, Thread.CurrentPrincipal, ct);
                await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(1));

                // The listener is called before the file is deleted, so there is a possible
                // race condition here.  Wait for a second to allow the delete to take place
                // before enumerating the files to see that they were actually deleted.
                await Task.Delay(TimeSpan.FromSeconds(1), ct);

                var messageEqualityComparer = new MessageEqualityComparer();
                mockListener.Verify(x =>
                    x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                        It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

                Assert.That(GetQueueDepth(queueName), Is.EqualTo(0));
                Assert.That(GetQueueDepth(queueName.GetRetryQueueName()), Is.EqualTo(0));
            }
            finally
            {
                rmqQueueingService.Dispose();
                cts.Dispose();
                DeleteQueue(queueName);
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
                var deadLetterExchange = queueName.GetDeadLetterExchangeName();
                var queueArgs = new Dictionary<string, object>
                {
                    {"x-dead-letter-exchange", deadLetterExchange}
                };
                channel.ExchangeDeclare(deadLetterExchange, "direct", true, false, null);
                channel.QueueDeclare(queueName, true, false, false, queueArgs);
                await RabbitMQHelper.PublishMessage(message, Thread.CurrentPrincipal, channel, queueName);
            }
        }

        private static void DeleteQueue(QueueName queueName)
        {
            var connectionFactory = new ConnectionFactory { Uri = RabbitMQUri.ToString() };
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // We have to declare the queue as a persistence queue because this is 
                // called before the queue is created by the RabbitMQQueueingService
                var queueExchange = queueName.GetExchangeName();
                var retryExchange = queueName.GetRetryExchangeName();
                var deadLetterExchange = queueName.GetDeadLetterExchangeName();
                var retryQueueName = queueName.GetRetryQueueName();

                channel.QueueDeleteNoWait(queueName, false, false);
                channel.QueueDeleteNoWait(retryQueueName, false, false);
                channel.ExchangeDeleteNoWait(queueExchange, false);
                channel.ExchangeDeleteNoWait(retryExchange, false);
                channel.ExchangeDeleteNoWait(deadLetterExchange, false);
            }
        }

        [Test]
        public async Task Given_Existing_Message_When_Creating_Queue_Then_Listener_Should_Fire()
        {
            var message = new Message(new MessageHeaders
                {
                    {HeaderName.ContentType, "text/plain"},
                    {HeaderName.MessageId, Guid.NewGuid().ToString()}
                }, "Hello, world!");

            var listenerCalledEvent = new ManualResetEvent(false);
            var queueName = new QueueName(Guid.NewGuid().ToString());
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

            var cts = new CancellationTokenSource();
            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            try
            {
                var ct = cts.Token;
                await StageExistingMessage(message, queueName);
                await rmqQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions {MaxAttempts = 1, RetryDelay = TimeSpan.FromSeconds(30)}, ct);
                await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(5));

                var messageEqualityComparer = new MessageEqualityComparer();
                mockListener.Verify(x =>
                    x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                        It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

                Assert.That(GetQueueDepth(queueName), Is.EqualTo(0));
                Assert.That(GetQueueDepth(queueName.GetRetryQueueName()), Is.EqualTo(0));
            }
            finally
            {
                rmqQueueingService.Dispose();
                cts.Dispose();
                DeleteQueue(queueName);
            }
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

            var newMessages = Enumerable.Range(11, 10)
                .Select(i => new Message(new MessageHeaders
                {
                    {HeaderName.ContentType, "text/plain"},
                    {HeaderName.MessageId, Guid.NewGuid().ToString()}
                }, "Hello, world! (" + i + ")"))
                .ToList();

            var listenerCountdown = new CountdownEvent(existingMessages.Count + newMessages.Count);

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x =>
                x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    ctx.Acknowledge();
                    listenerCountdown.Signal();
                })
                .Returns(Task.FromResult(true));

            var queueName = new QueueName(Guid.NewGuid().ToString());
            var cts = new CancellationTokenSource();
            var rmqQueueingService = new RabbitMQMessageQueueingService(RabbitMQUri);
            try
            {
                var ct = cts.Token;
                var stagingTasks = existingMessages.Select(msg => StageExistingMessage(msg, queueName)).ToList();
                await Task.WhenAll(stagingTasks);
                await rmqQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions {MaxAttempts = 1}, ct);

                var enqueueingTasks = newMessages
                    .Select(msg => rmqQueueingService.EnqueueMessage(queueName, msg, Thread.CurrentPrincipal, ct))
                    .ToList();

                await Task.WhenAll(enqueueingTasks);

                var timedOut = !await listenerCountdown.WaitHandle.WaitOneAsync(TimeSpan.FromSeconds(10));

                Assert.That(timedOut, Is.False, "Timed out waiting for listeners to be called");

                var messageEqualityComparer = new MessageEqualityComparer();
                var allmessages = existingMessages.Union(newMessages);
                foreach (var message in allmessages)
                {
                    mockListener.Verify(x =>
                        x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                            It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());
                }

                Assert.That(GetQueueDepth(queueName), Is.EqualTo(0));
                Assert.That(GetQueueDepth(queueName.GetRetryQueueName()), Is.EqualTo(0));
            }
            finally
            {
                rmqQueueingService.Dispose();
                cts.Dispose();
                DeleteQueue(queueName);
            }
        }
    }
}