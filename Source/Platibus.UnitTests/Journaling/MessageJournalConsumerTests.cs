using Platibus.Config;
using Platibus.Journaling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Platibus.UnitTests.Journaling
{
    public class MessageJournalConsumerTests : IMessageHandler<string>
    {
        protected readonly object SyncRoot = new object();

        protected MessageJournalStub MessageJournal;
        protected PlatibusConfiguration Configuration;
        protected MessageJournalConsumerOptions Options;
        protected MessageJournalConsumer MessageJournalConsumer;
        protected MessageJournalPosition Start;
        protected Task ConsumeTask;

        protected IList<Message> JournaledMessages = new List<Message>();
        protected int ExpectedMessageCount;
        protected bool Acknowledge;
        protected Exception Exception;

        protected IList<Message> ConsumedMessages = new List<Message>();
        protected TaskCompletionSource<IEnumerable<Message>> ConsumedMessageSource;
        protected CancellationTokenSource CancellationSource;
        protected CancellationToken CancellationToken;

        public MessageJournalConsumerTests()
        {
            MessageJournal = new MessageJournalStub();
            Configuration = new PlatibusConfiguration
            {
                MessageJournal = MessageJournal
            };
            Options = new MessageJournalConsumerOptions();

            var anyMessage = new DelegateMessageSpecification(m => true);
            var thisHandler = new GenericMessageHandlerAdapter<string>(this);
            Configuration.AddHandlingRule(new HandlingRule(anyMessage, thisHandler));

            ConsumedMessageSource = new TaskCompletionSource<IEnumerable<Message>>();
            CancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            CancellationSource.Token.Register(() => ConsumedMessageSource.TrySetCanceled());

            CancellationToken = CancellationSource.Token;
        }

        [Fact]
        public async Task MessageJournalConsumerConsumesMessages()
        {
            await GivenJournaledMessage();
            GivenMessageHandlerAcknowledgesMessage();
            WhenConsumingJournaledMessages();
            AssertMessagesAreConsumed();
            CancellationSource.Cancel();
            AssertMessageJournalConsumerHasStopped();
        }

        [Fact]
        public async Task MessageJournalConsumerConsumesAllMessages()
        {
            for (var i = 0; i < 10; i++)
            {
                await GivenJournaledMessage(i);
            }
            GivenMessageHandlerAcknowledgesMessage();
            WhenConsumingJournaledMessages();
            AssertMessagesAreConsumed();
            CancellationSource.Cancel();
            AssertMessageJournalConsumerHasStopped();
        }

        [Fact]
        public async Task MessageJournalConsumerReportsProgress()
        {
            var progress = new ProgressStub(CancellationToken);
            Options.Progress = progress;

            await GivenJournaledMessage();
            GivenMessageHandlerAcknowledgesMessage();
            WhenConsumingJournaledMessages();
            AssertMessagesAreConsumed();

            var progressReport = await progress.Report;
            Assert.NotNull(progressReport);
            Assert.Equal(1, progressReport.Count);
            Assert.Equal(new MessageJournalStub.Position(0), progressReport.Current);
            Assert.Equal(new MessageJournalStub.Position(1), progressReport.Next);

            CancellationSource.Cancel();
            AssertMessageJournalConsumerHasStopped();
        }

        [Fact]
        public async Task MessageJournalConsumerCanBeConfiguredToRethrowMessageNotAcknowledgedException()
        {
            await GivenJournaledMessage();
            GivenMessageHandlerDoesNotAcknowledgeMessage();
            Options.RethrowExceptions = true;
            WhenConsumingJournaledMessages();
            await Assert.ThrowsAsync<MessageNotAcknowledgedException>(async () => await ConsumeTask);
            AssertMessagesAreConsumed();
            AssertMessageJournalConsumerHasStopped();
        }

        [Fact]
        public async Task MessageJournalConsumerCanBeConfiguredToIgnoreMessageNotAcknowledgedException()
        {
            await GivenJournaledMessage();
            GivenMessageHandlerDoesNotAcknowledgeMessage();
            Options.RethrowExceptions = false;
            WhenConsumingJournaledMessages();
            CancellationSource.Cancel();
            AssertMessagesAreConsumed();
            AssertMessageJournalConsumerHasStopped();
        }

        [Fact]
        public async Task MessageJournalConsumerCanBeConfiguredToRethrowHandlerExceptions()
        {
            await GivenJournaledMessage();
            var expectedException = new Exception("Test");
            GivenMessageHandlerThrowsException(expectedException);
            Options.RethrowExceptions = true;
            WhenConsumingJournaledMessages();
            var actualException = await Assert.ThrowsAsync<Exception>(async () => await ConsumeTask);
            Assert.Equal(expectedException, actualException);
            AssertMessagesAreConsumed();
            AssertMessageJournalConsumerHasStopped();
        }

        [Fact]
        public async Task MessageJournalConsumerCanBeConfiguredToIgnoreHandlerExceptions()
        {
            await GivenJournaledMessage();
            var expectedException = new Exception("Test");
            GivenMessageHandlerThrowsException(expectedException);
            Options.RethrowExceptions = false;
            WhenConsumingJournaledMessages();
            AssertMessagesAreConsumed();
            AssertMessageJournalConsumerIsRunning();
            CancellationSource.Cancel();
            AssertMessageJournalConsumerHasStopped();
        }

        [Fact]
        public async Task MessageJournalConsumerStopsWhenCancellationRequested()
        {
            await GivenJournaledMessage();
            WhenConsumingJournaledMessages();
            AssertMessagesAreConsumed();
            CancellationSource.Cancel();
            AssertMessageJournalConsumerHasStopped();
        }

        [Fact]
        public async Task MessageJournalConsumerCanBeConfiguredToStopAtEndOfJournal()
        {
            await GivenJournaledMessage();
            Options.HaltAtEndOfJournal = true;
            WhenConsumingJournaledMessages();
            AssertMessagesAreConsumed();
            AssertMessageJournalConsumerHasStopped();
        }

        [Fact]
        public async Task MessageJournalConsumerCanBeConfiguredToContinuePollingWhenEndOfJournalReached()
        {
            await GivenJournaledMessage();
            Options.HaltAtEndOfJournal = false;
            WhenConsumingJournaledMessages();
            AssertMessagesAreConsumed();
            WaitForPollingInterval();
            AssertMessageJournalConsumerIsRunning();
            CancellationSource.Cancel();
        }

        private void WaitForPollingInterval()
        {
            Task.Delay(Options.PollingInterval, CancellationToken).Wait(CancellationToken);
        }

        protected async Task GivenJournaledMessage(int i = 0)
        {
            var headers = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                Topic = "MessageJournalConsumerTests",
                ContentType = "text/plain",
                Published = DateTime.UtcNow
            };
            var message = new Message(headers, $"Test {i + 1}");
            await MessageJournal.Append(message, MessageJournalCategory.Published, CancellationToken);
            lock (SyncRoot)
            {
                JournaledMessages.Add(message);
                ExpectedMessageCount++;
            }
        }

        protected void GivenMessageHandlerAcknowledgesMessage() => Acknowledge = true;

        protected void GivenMessageHandlerDoesNotAcknowledgeMessage() => Acknowledge = false;

        protected void GivenMessageHandlerThrowsException(Exception ex) => Exception = ex;

        protected void WhenConsumingJournaledMessages()
        {
            MessageJournalConsumer = new MessageJournalConsumer(Configuration, Options);
            ConsumeTask = MessageJournalConsumer.Consume(Start, CancellationToken);
        }

        protected void AssertMessageJournalConsumerHasStopped()
        {
            var completionSource = new TaskCompletionSource<bool>();
            ConsumeTask.ContinueWith(t => completionSource.TrySetResult(true));
            Assert.True(completionSource.Task.Wait(TimeSpan.FromSeconds(5)));
        }

        protected void AssertMessageJournalConsumerIsRunning()
        {
            Assert.False(ConsumeTask.IsCompleted || ConsumeTask.IsCanceled || ConsumeTask.IsFaulted);
        }

        protected void AssertMessagesAreConsumed()
        {
            var consumedMessages = ConsumedMessageSource.Task.Result?.ToList();
            Assert.NotNull(consumedMessages);
            Assert.NotEmpty(consumedMessages);
            Assert.Equal(ExpectedMessageCount, ConsumedMessages.Count);
            Assert.Equal(JournaledMessages, ConsumedMessages, new MessageEqualityComparer());
        }

        Task IMessageHandler<string>.HandleMessage(string content, IMessageContext messageContext, CancellationToken cancellationToken)
        {
            var message = new Message(messageContext.Headers, content);
            lock (SyncRoot)
            {
                ConsumedMessages.Add(message);
                var allMessagesConsumed = ConsumedMessages.Count == ExpectedMessageCount;
                if (allMessagesConsumed)
                {
                    ConsumedMessageSource.TrySetResult(ConsumedMessages);
                }
            }
            
            if (Exception != null) throw Exception;
            if (Acknowledge)
            {
                messageContext.Acknowledge();
            }

            return Task.FromResult(0);
        }
    }
}
