using Platibus.Config;
using Platibus.Journaling;
using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Platibus.UnitTests.Journaling
{
    public class MessageJournalConsumerTests : IMessageHandler<string>
    {
        protected MessageJournalStub MessageJournal;
        protected PlatibusConfiguration Configuration;
        protected MessageJournalConsumerOptions Options;
        protected MessageJournalConsumer MessageJournalConsumer;
        protected MessageJournalPosition Start;
        protected Task ConsumeTask;

        protected Message Message;
        protected bool Acknowledge;
        protected Exception Exception;

        protected TaskCompletionSource<Message> ConsumedMessageSource;
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

            ConsumedMessageSource = new TaskCompletionSource<Message>();
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
            AssertMessageConsumed();
            CancellationSource.Cancel();
            AssertMessageJournalConsumerHasStopped();
        }

        [Fact]
        public async Task MessageJournalConsumerReportsProgress()
        {
            var progress = new ProgressStub();
            Options.Progress = progress;

            await GivenJournaledMessage();
            GivenMessageHandlerAcknowledgesMessage();
            WhenConsumingJournaledMessages();
            AssertMessageConsumed();
            CancellationSource.Cancel();
            AssertMessageJournalConsumerHasStopped();

            Assert.NotNull(progress.Report);
        }

        [Fact]
        public async Task MessageJournalConsumerCanBeConfiguredToRethrowMessageNotAcknowledgedException()
        {
            await GivenJournaledMessage();
            GivenMessageHandlerDoesNotAcknowledgeMessage();
            Options.RethrowExceptions = true;
            WhenConsumingJournaledMessages();
            await Assert.ThrowsAsync<MessageNotAcknowledgedException>(async () => await ConsumeTask);
            AssertMessageConsumed();
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
            AssertMessageConsumed();
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
            AssertMessageConsumed();
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
            AssertMessageConsumed();
            AssertMessageJournalConsumerIsRunning();
            CancellationSource.Cancel();
            AssertMessageJournalConsumerHasStopped();
        }

        [Fact]
        public async Task MessageJournalConsumerStopsWhenCancellationRequested()
        {
            await GivenJournaledMessage();
            WhenConsumingJournaledMessages();
            AssertMessageConsumed();
            CancellationSource.Cancel();
            AssertMessageJournalConsumerHasStopped();
        }

        [Fact]
        public async Task MessageJournalConsumerCanBeConfiguredToStopAtEndOfJournal()
        {
            await GivenJournaledMessage();
            Options.HaltAtEndOfJournal = true;
            WhenConsumingJournaledMessages();
            AssertMessageConsumed();
            AssertMessageJournalConsumerHasStopped();
        }

        [Fact]
        public async Task MessageJournalConsumerCanBeConfiguredToContinuePollingWhenEndOfJournalReached()
        {
            await GivenJournaledMessage();
            Options.HaltAtEndOfJournal = false;
            WhenConsumingJournaledMessages();
            AssertMessageConsumed();
            WaitForPollingInterval();
            AssertMessageJournalConsumerIsRunning();
            CancellationSource.Cancel();
        }

        private void WaitForPollingInterval()
        {
            Task.Delay(Options.PollingInterval, CancellationToken).Wait(CancellationToken);
        }

        protected async Task GivenJournaledMessage()
        {
            var headers = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                Topic = "MessageJournalConsumerTests",
                ContentType = "text/plain",
                Published = DateTime.UtcNow
            };
            Message = new Message(headers, "Test");
            await MessageJournal.Append(Message, MessageJournalCategory.Published, CancellationToken);
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

        protected void AssertMessageConsumed()
        {
            var consumedMessage = ConsumedMessageSource.Task.Result;
            Assert.NotNull(consumedMessage);
            Assert.Equal(Message, consumedMessage, new MessageEqualityComparer());
        }

        Task IMessageHandler<string>.HandleMessage(string content, IMessageContext messageContext, CancellationToken cancellationToken)
        {
            var message = new Message(messageContext.Headers, content);
            ConsumedMessageSource.TrySetResult(message);
            if (Exception != null) throw Exception;
            if (Acknowledge)
            {
                messageContext.Acknowledge();
            }

            return Task.FromResult(0);
        }
    }
}
