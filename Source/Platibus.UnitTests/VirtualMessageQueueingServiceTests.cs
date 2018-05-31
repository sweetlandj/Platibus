using Platibus.Queueing;
using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Platibus.UnitTests
{
    [Trait("Category", "UnitTests")]
    public class VirtualMessageQueueingServiceTests
    {
        protected readonly VirtualMessageQueueingService MessageQueueingService = new VirtualMessageQueueingService();

        protected IPrincipal Principal;
        protected QueueListenerStub Listener;
        protected Message HandledMessage;

        [Fact]
        public async Task QueueListenerFiresWhenNewMessageEnqueued()
        {
            var listener = new QueueListenerStub();

            var queue = GivenUniqueQueueName();
            var message = GivenSampleMessage();
            await MessageQueueingService.CreateQueue(queue, listener);
            await MessageQueueingService.EnqueueMessage(queue, message, null);
            await listener.Completed;
            listener.Dispose();
            AssertMessageHandled(message, listener.Message);
        }

        [Fact]
        public async Task PrincipalIsPreservedWhenListenerInvoked()
        {
            var listener = new QueueListenerStub();
            GivenClaimsPrincipal();
            var queue = GivenUniqueQueueName();
            var message = GivenSampleMessage();
            await MessageQueueingService.CreateQueue(queue, listener);
            await MessageQueueingService.EnqueueMessage(queue, message, Principal);
            await listener.Completed;
            listener.Dispose();
            AssertPrincipalPreserved(listener.Context.Principal);
        }

        [Fact]
        public async Task MessageCanBeAutomaticallyAcknowledged()
        {
            var queue = GivenUniqueQueueName();
            var message = GivenSampleMessage();
            var listener = new QueueListenerStub((m, c) => { });
            var autoAcknowledgeOptions = new QueueOptions
            {
                AutoAcknowledge = true,
                IsDurable = false
            };
            await MessageQueueingService.CreateQueue(queue, listener, autoAcknowledgeOptions);
            await MessageQueueingService.EnqueueMessage(queue, message, Principal);
            await listener.Completed;
            listener.Dispose();

            await QueueOperationCompletion();
        }

        [Fact]
        public async Task EnqueueThrowsIfMessageNotAcknowledged()
        {
            var queue = GivenUniqueQueueName();
            var message = GivenSampleMessage();
            var listener = new QueueListenerStub((m, c) => {});
            await MessageQueueingService.CreateQueue(queue, listener);
            await Assert.ThrowsAsync<MessageNotAcknowledgedException>(() => MessageQueueingService.EnqueueMessage(queue, message, Principal));
            await listener.Completed;
            listener.Dispose();

            await QueueOperationCompletion();
        }
        
        [Fact]
        public async Task MessageNotAutomaticallyAcknowledgedWhenListenerThrows()
        {
            var queue = GivenUniqueQueueName();
            var message = GivenSampleMessage();
            var listener = new QueueListenerStub((m, c) => throw new Exception("Test"));
            var autoAcknowledgeOptions = new QueueOptions
            {
                AutoAcknowledge = true,
                IsDurable = false
            };
            await MessageQueueingService.CreateQueue(queue, listener, autoAcknowledgeOptions);
            var ex = await Assert.ThrowsAsync<Exception>(() => MessageQueueingService.EnqueueMessage(queue, message, Principal));
            await listener.Completed;
            listener.Dispose();

            Assert.Equal("Test", ex.Message);

            await QueueOperationCompletion();
        }
        
        protected QueueName GivenUniqueQueueName()
        {
            return Guid.NewGuid().ToString("N");
        }
        
        protected IPrincipal GivenClaimsPrincipal()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "test@example.com"),
                new Claim(ClaimTypes.Role, "user"),
                new Claim(ClaimTypes.Role, "staff"),
            };
            return Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Basic"));
        }

        protected virtual Task QueueOperationCompletion()
        {
            return Task.Delay(100);
        }

        protected void AssertMessageHandled(Message originalMessage, Message handledMessage)
        {
            var comparer = new MessageEqualityComparer(HeaderName.SecurityToken);
            Assert.NotNull(handledMessage);
            Assert.Equal(originalMessage, handledMessage, comparer);
        }

        protected void AssertPrincipalPreserved(IPrincipal contextPrincipal)
        {
            if (Principal == null) Assert.Null(contextPrincipal);
            Assert.NotNull(contextPrincipal);

            var originalClaimsPrincipal = Principal as ClaimsPrincipal;
            Assert.NotNull(originalClaimsPrincipal);

            var contextClaimsPrincipal = contextPrincipal as ClaimsPrincipal;
            Assert.NotNull(contextClaimsPrincipal);
            var originalClaims = originalClaimsPrincipal.Claims;
            foreach (var originalClaim in originalClaims)
            {
                Assert.True(contextClaimsPrincipal.HasClaim(originalClaim.Type, originalClaim.Value));
            }
        }

        protected static Message GivenSampleMessage(Action<MessageHeaders> setCustomHeaders = null)
        {
            var headers = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                Origination = new Uri("http://localhost/platibus0"),
                Destination = new Uri("http://localhost/platibus1"),
                ContentType = "text/plain",
                Expires = DateTime.UtcNow.AddMinutes(5),
                MessageName = "http://example.com/ns/test",
                Sent = DateTime.UtcNow
            };

            setCustomHeaders?.Invoke(headers);

            return new Message(headers, "Hello, world!");
        }

        protected class QueueListenerStub : IQueueListener, IDisposable
        {
            private readonly Action<Message, IQueuedMessageContext> _callback;
            private readonly TaskCompletionSource<Message> _taskCompletionSource;
            private readonly CancellationTokenSource _cancellationTokenSource;

            private bool _disposed;

            public Task Completed => _taskCompletionSource.Task;

            public CancellationToken CancellationToken => _cancellationTokenSource.Token;

            public Message Message { get; private set; }
            public IQueuedMessageContext Context { get; private set; }

            public QueueListenerStub(Action<Message, IQueuedMessageContext> callback = null, TimeSpan timeout = default(TimeSpan))
            {
                _callback = callback ?? ((m, c) => c.Acknowledge());

                _taskCompletionSource = new TaskCompletionSource<Message>();
                _cancellationTokenSource = new CancellationTokenSource();
                var cancelAfter = timeout <= TimeSpan.Zero
                    ? TimeSpan.FromSeconds(5)
                    : timeout;

                _cancellationTokenSource.Token.Register(() => _taskCompletionSource.TrySetCanceled());
                _cancellationTokenSource.CancelAfter(cancelAfter);
            }

            public Task MessageReceived(Message message, IQueuedMessageContext context,
                CancellationToken cancellationToken = new CancellationToken())
            {
                Message = message;
                Context = context;
                try
                {
                    _callback(message, context);
                    return Task.FromResult(0);
                }
                finally
                {
                    _taskCompletionSource.TrySetResult(message);
                }
            }

            public void Dispose()
            {
                if (_disposed) return;
                Dispose(true);
                _disposed = true;
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                _cancellationTokenSource.Cancel();
                if (disposing)
                {
                    _cancellationTokenSource.Dispose();
                }
            }
        }
    }
}