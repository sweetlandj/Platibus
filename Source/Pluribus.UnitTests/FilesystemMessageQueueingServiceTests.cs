using Moq;
using NUnit.Framework;
using Pluribus.Filesystem;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pluribus.UnitTests
{
    class FilesystemMessageQueueingServiceTests
    {
        protected DirectoryInfo GetTempDirectory()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "Pluribus.UnitTests", DateTime.Now.ToString("yyyyMMddHHmmss"));
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
            var listenerCalledEvent = new ManualResetEvent(false);
            var tempDir = GetTempDirectory();
            var fsQueueingService = new FilesystemMessageQueueingService(tempDir);
            fsQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x => x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    ctx.Acknowledge();
                    listenerCalledEvent.Set();
                })
                .Returns(Task.FromResult(true));

            var queueName = new QueueName(Guid.NewGuid().ToString());
            await fsQueueingService
                .CreateQueue(queueName, mockListener.Object, new QueueOptions { MaxAttempts = 1 })
                .ConfigureAwait(false);

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            await fsQueueingService
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
            var tempDir = GetTempDirectory();
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var queuePath = Path.Combine(tempDir.FullName, queueName);
            var queueDir = new DirectoryInfo(queuePath);
            if (!queueDir.Exists)
            {
                queueDir.Create();
            }

            var fsQueueingService = new FilesystemMessageQueueingService(tempDir);
            fsQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x => x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    ctx.Acknowledge();
                    listenerCalledEvent.Set();
                })
                .Returns(Task.FromResult(true));

            await fsQueueingService
                .CreateQueue(queueName, mockListener.Object, new QueueOptions { MaxAttempts = 1 })
                .ConfigureAwait(false);

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            await fsQueueingService
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

            var queuedMessages = queueDir.EnumerateFiles()
                .Select(f => new MessageFile(f))
                .ToList();

            Assert.That(queuedMessages, Is.Empty);
        }

        [Test]
        public async Task Given_Queued_Message_When_Not_Acknowledged_Then_Message_Should_Not_Be_Deleted()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var tempDir = GetTempDirectory();
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var queuePath = Path.Combine(tempDir.FullName, queueName);
            var queueDir = new DirectoryInfo(queuePath);
            if (!queueDir.Exists)
            {
                queueDir.Create();
            }

            var fsQueueingService = new FilesystemMessageQueueingService(tempDir);
            fsQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x => x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    listenerCalledEvent.Set();
                })
                .Returns(Task.FromResult(true));

            await fsQueueingService
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

            await fsQueueingService
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

            var queuedMessages = queueDir.EnumerateFiles()
                .Select(f => new MessageFile(f))
                .ToList();

            Assert.That(queuedMessages.Count, Is.EqualTo(1));
            Assert.That(await queuedMessages[0].ReadMessage(), Is.EqualTo(message).Using(messageEqualityComparer));
        }

        [Test]
        public async Task Given_Auto_Acknowledge_Queue_When_Not_Acknowledged_Then_Message_Should_Be_Deleted()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var tempDir = GetTempDirectory();
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var queuePath = Path.Combine(tempDir.FullName, queueName);
            var queueDir = new DirectoryInfo(queuePath);
            if (!queueDir.Exists)
            {
                queueDir.Create();
            }

            var fsQueueingService = new FilesystemMessageQueueingService(tempDir);
            fsQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x => x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    listenerCalledEvent.Set();
                })
                .Returns(Task.FromResult(true));

            await fsQueueingService
                .CreateQueue(queueName, mockListener.Object, new QueueOptions { AutoAcknowledge = true })
                .ConfigureAwait(false);

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            await fsQueueingService
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

            var queuedMessages = queueDir.EnumerateFiles()
                .Select(f => new MessageFile(f))
                .ToList();

            Assert.That(queuedMessages, Is.Empty);
        }

        [Test]
        public async Task Given_Auto_Acknowledge_Queue_When_Listener_Throws_Then_Message_Should_Not_Be_Deleted()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var tempDir = GetTempDirectory();
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var queuePath = Path.Combine(tempDir.FullName, queueName);
            var queueDir = new DirectoryInfo(queuePath);
            if (!queueDir.Exists)
            {
                queueDir.Create();
            }

            var fsQueueingService = new FilesystemMessageQueueingService(tempDir);
            fsQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x => x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    listenerCalledEvent.Set();
                    throw new Exception();
                });

            await fsQueueingService
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

            await fsQueueingService
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

            var queuedMessages = queueDir.EnumerateFiles()
                .Select(f => new MessageFile(f))
                .ToList();

            Assert.That(queuedMessages.Count, Is.EqualTo(1));
            Assert.That(await queuedMessages[0].ReadMessage(), Is.EqualTo(message).Using(messageEqualityComparer));
        }

        [Test]
        public async Task Given_Existing_Message_When_Creating_Queue_Then_Listener_Should_Fire()
        {
            var listenerCalledEvent = new ManualResetEvent(false);
            var tempDir = GetTempDirectory();
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var queuePath = Path.Combine(tempDir.FullName, queueName);
            var queueDir = new DirectoryInfo(queuePath);
            if (!queueDir.Exists)
            {
                queueDir.Create();
            }

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()}
            }, "Hello, world!");

            await MessageFile.Create(queueDir, message, Thread.CurrentPrincipal)
                .ConfigureAwait(false);

            var fsQueueingService = new FilesystemMessageQueueingService(tempDir);
            fsQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x => x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    ctx.Acknowledge();
                    listenerCalledEvent.Set();
                })
                .Returns(Task.FromResult(true));

            await fsQueueingService
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

            var tempDir = GetTempDirectory();
            var queueName = new QueueName(Guid.NewGuid().ToString());
            var queuePath = Path.Combine(tempDir.FullName, queueName);
            var queueDir = new DirectoryInfo(queuePath);
            if (!queueDir.Exists)
            {
                queueDir.Create();
            }

            await Task.WhenAll(existingMessages.Select(msg => MessageFile.Create(queueDir, msg, Thread.CurrentPrincipal)))
                .ConfigureAwait(false);

            var newMessages = Enumerable.Range(1, 10)
                .Select(i => new Message(new MessageHeaders
                {
                    {HeaderName.ContentType, "text/plain"},
                    {HeaderName.MessageId, Guid.NewGuid().ToString()}
                }, "Hello, world! (" + i + ")"))
                .ToList();

            var listenerCountdown = new CountdownEvent(existingMessages.Count + newMessages.Count);
            
            var fsQueueingService = new FilesystemMessageQueueingService(tempDir);
            fsQueueingService.Init();

            var mockListener = new Mock<IQueueListener>();
            mockListener.Setup(x => x.MessageReceived(It.IsAny<Message>(), It.IsAny<IQueuedMessageContext>(), It.IsAny<CancellationToken>()))
                .Callback<Message, IQueuedMessageContext, CancellationToken>((msg, ctx, ct) =>
                {
                    ctx.Acknowledge();
                    listenerCountdown.Signal();
                })
                .Returns(Task.FromResult(true));

            await fsQueueingService
                .CreateQueue(queueName, mockListener.Object, new QueueOptions { MaxAttempts = 1 })
                .ConfigureAwait(false);

            await Task.WhenAll(newMessages.Select(msg => fsQueueingService.EnqueueMessage(queueName, msg, Thread.CurrentPrincipal)))
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
