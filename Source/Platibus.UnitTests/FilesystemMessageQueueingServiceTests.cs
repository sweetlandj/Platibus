using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Platibus.Filesystem;

namespace Platibus.UnitTests
{
    internal class FilesystemMessageQueueingServiceTests
    {
        protected DirectoryInfo GetTempDirectory()
        {
            var ts = DateTime.Now.ToString("yyyyMMddHHmmss");
            var tempPath = Path.Combine(Path.GetTempPath(), "Platibus.UnitTests", ts);
            var tempDir = new DirectoryInfo(tempPath);
            if (!tempDir.Exists)
            {
                tempDir.Create();
            }
            return tempDir;
        }

        [Test]
        public async Task Given_Existing_Queue_When_New_Message_Queued_Then_Listener_Should_Fire()
        {
            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            var listenerCalledEvent = new ManualResetEvent(false);
            var tempDir = GetTempDirectory();
           
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

            using (var fsQueueingService = new FilesystemMessageQueueingService(tempDir))
            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                fsQueueingService.Init();

                await fsQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions { MaxAttempts = 1 }, ct);
                await fsQueueingService.EnqueueMessage(queueName, message, Thread.CurrentPrincipal, ct);
                await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(1));
            }

            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(x =>
                x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                    It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());
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
            var tempDir = GetTempDirectory();
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var queuePath = Path.Combine(tempDir.FullName, queueName);
            var queueDir = new DirectoryInfo(queuePath);
            if (!queueDir.Exists)
            {
                queueDir.Create();
            }

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

            using (var fsQueueingService = new FilesystemMessageQueueingService(tempDir))
            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                fsQueueingService.Init();

                await fsQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions { MaxAttempts = 1 }, ct);
                await fsQueueingService.EnqueueMessage(queueName, message, Thread.CurrentPrincipal, ct);
                await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(1));

                // The listener is called before the file is deleted, so there is a possible
                // race condition here.  Wait for a second to allow the delete to take place
                // before enumerating the files to see that they were actually deleted.
                await Task.Delay(TimeSpan.FromSeconds(1), ct);
            }

            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(x =>
                x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                    It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

            var queuedMessages = queueDir.EnumerateFiles()
                .Select(f => new MessageFile(f))
                .ToList();

            Assert.That(queuedMessages, Is.Empty);
        }

        [Test]
        public async Task Given_Queued_Message_When_Not_Acknowledged_Then_Message_Should_Not_Be_Deleted()
        {
            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            var listenerCalledEvent = new ManualResetEvent(false);
            var tempDir = GetTempDirectory();
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var queuePath = Path.Combine(tempDir.FullName, queueName);
            var queueDir = new DirectoryInfo(queuePath);
            if (!queueDir.Exists)
            {
                queueDir.Create();
            }
            
            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x =>
                x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>(
                    (msg, ctx, ct) => { listenerCalledEvent.Set(); })
                .Returns(Task.FromResult(true));

            using (var fsQueueingService = new FilesystemMessageQueueingService(tempDir))
            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                fsQueueingService.Init();
                await fsQueueingService
                    .CreateQueue(queueName, mockListener.Object, new QueueOptions
                    {
                        MaxAttempts = 2, // Prevent message from being sent to the DLQ,
                        RetryDelay = TimeSpan.FromSeconds(30)
                    }, ct);

                await fsQueueingService.EnqueueMessage(queueName, message, Thread.CurrentPrincipal, ct);
                await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(1));

                // The listener is called before the file is deleted, so there is a possible
                // race condition here.  Wait for a second to allow the delete to take place
                // before enumerating the files to see that they were actually not deleted.
                await Task.Delay(TimeSpan.FromSeconds(1), ct);
            }
            
            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(x =>
                x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                    It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

            var queuedMessages = queueDir.EnumerateFiles()
                .Select(f => new MessageFile(f))
                .ToList();

            Assert.That(queuedMessages.Count, Is.EqualTo(1));
            using (var cts = new CancellationTokenSource())
            {
                Assert.That(await queuedMessages[0].ReadMessage(cts.Token),
                    Is.EqualTo(message).Using(messageEqualityComparer));
            }
        }

        [Test]
        public async Task Given_Auto_Acknowledge_Queue_When_Not_Acknowledged_Then_Message_Should_Be_Deleted()
        {
            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            var listenerCalledEvent = new ManualResetEvent(false);
            var tempDir = GetTempDirectory();
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var queuePath = Path.Combine(tempDir.FullName, queueName);
            var queueDir = new DirectoryInfo(queuePath);
            if (!queueDir.Exists)
            {
                queueDir.Create();
            }
            
            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(
                x =>
                    x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(),
                        It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>(
                    (msg, ctx, ct) => { listenerCalledEvent.Set(); })
                .Returns(Task.FromResult(true));

            using (var fsQueueingService = new FilesystemMessageQueueingService(tempDir))
            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                fsQueueingService.Init();

                await fsQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions { AutoAcknowledge = true }, ct);
                await fsQueueingService.EnqueueMessage(queueName, message, Thread.CurrentPrincipal, ct);
                await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(1));

                // The listener is called before the file is deleted, so there is a possible
                // race condition here.  Wait for a second to allow the delete to take place
                // before enumerating the files to see that they were actually deleted.
                await Task.Delay(TimeSpan.FromSeconds(1), ct);
            }

            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(x =>
                x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                    It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

            var queuedMessages = queueDir.EnumerateFiles()
                .Select(f => new MessageFile(f))
                .ToList();

            Assert.That(queuedMessages, Is.Empty);
        }

        [Test]
        public async Task Given_Auto_Acknowledge_Queue_When_Listener_Throws_Then_Message_Should_Not_Be_Deleted()
        {
            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            var listenerCalledEvent = new ManualResetEvent(false);
            var tempDir = GetTempDirectory();
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var queuePath = Path.Combine(tempDir.FullName, queueName);
            var queueDir = new DirectoryInfo(queuePath);
            if (!queueDir.Exists)
            {
                queueDir.Create();
            }
            
            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(
                x =>
                    x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(),
                        It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    listenerCalledEvent.Set();
                    throw new Exception();
                });

            using (var fsQueueingService = new FilesystemMessageQueueingService(tempDir))
            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                fsQueueingService.Init();

                await fsQueueingService
                    .CreateQueue(queueName, mockListener.Object, new QueueOptions
                    {
                        AutoAcknowledge = true,
                        MaxAttempts = 2, // So the message doesn't get moved to the DLQ
                        RetryDelay = TimeSpan.FromSeconds(30)
                    }, ct);

                await fsQueueingService.EnqueueMessage(queueName, message, Thread.CurrentPrincipal, ct);

                var listenerCalled = await listenerCalledEvent
                    .WaitOneAsync(Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromSeconds(3));

                Assert.That(listenerCalled, Is.True);

                // The listener is called before the file is deleted, so there is a possible
                // race condition here.  Wait for a second to allow the delete to take place
                // before enumerating the files to see that they were actually not deleted.
                await Task.Delay(TimeSpan.FromSeconds(1), ct);
            }

            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(x =>
                x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                    It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());

            var queuedMessages = queueDir.EnumerateFiles()
                .Select(f => new MessageFile(f))
                .ToList();

            Assert.That(queuedMessages.Count, Is.EqualTo(1));
            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                Assert.That(await queuedMessages[0].ReadMessage(ct), Is.EqualTo(message).Using(messageEqualityComparer));
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
            var tempDir = GetTempDirectory();
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var queuePath = Path.Combine(tempDir.FullName, queueName);
            var queueDir = new DirectoryInfo(queuePath);
            if (!queueDir.Exists)
            {
                queueDir.Create();
            }
            
            await MessageFile.Create(queueDir, message, Thread.CurrentPrincipal);
            
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

            using (var fsQueueingService = new FilesystemMessageQueueingService(tempDir))
            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                fsQueueingService.Init();
                await fsQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions { MaxAttempts = 1 }, ct);
                await listenerCalledEvent.WaitOneAsync(TimeSpan.FromSeconds(1));
            }

            var messageEqualityComparer = new MessageEqualityComparer();
            mockListener.Verify(x =>
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

            var tempDir = GetTempDirectory();
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var queuePath = Path.Combine(tempDir.FullName, queueName);
            var queueDir = new DirectoryInfo(queuePath);
            if (!queueDir.Exists)
            {
                queueDir.Create();
            }

            await Task.WhenAll(existingMessages.Select(msg => MessageFile.Create(queueDir, msg, Thread.CurrentPrincipal)));

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

            using (var fsQueueingService = new FilesystemMessageQueueingService(tempDir))
            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                fsQueueingService.Init();

                await fsQueueingService.CreateQueue(queueName, mockListener.Object, new QueueOptions { MaxAttempts = 1 }, ct);
                var tasks = newMessages
                    .Select(msg => fsQueueingService.EnqueueMessage(queueName, msg, Thread.CurrentPrincipal, ct))
                    .ToList();
                await Task.WhenAll(tasks);

                var timedOut = !await listenerCountdown.WaitHandle.WaitOneAsync(TimeSpan.FromSeconds(10));

                Assert.That(timedOut, Is.False, "Timed out waiting for listeners to be called");
            }

            var messageEqualityComparer = new MessageEqualityComparer();
            var allmessages = existingMessages.Union(newMessages);
            foreach (var message in allmessages)
            {
                mockListener.Verify(x =>
                    x.MessageReceived(It.Is<Message>(m => messageEqualityComparer.Equals(m, message)),
                        It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()), Times.Once());
            }
        }
    }
}