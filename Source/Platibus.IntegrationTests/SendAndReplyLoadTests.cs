using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Platibus.IntegrationTests
{
    [Trait("Category", "LoadTests")]
    public abstract class SendAndReplyLoadTests
    {
        protected readonly ITestOutputHelper Output;
        protected readonly IBus Sender;
        protected int Count;
        protected TimeSpan Timeout;
        protected SendOptions SendOptions = new SendOptions {Synchronous = true};

        protected SendAndReplyLoadTests(IBus sender, ITestOutputHelper output)
        {
            Sender = sender;
            Output = output;
        }

        [Theory]
        [InlineData(1000, 5)]
        public void RepliesToConcurrentMessagesAreReceivedWithinAlottedTime(int count, int timeoutInSeconds)
        {
            GivenMessageCount(count);
            GivenTimeout(TimeSpan.FromSeconds(timeoutInSeconds));
            WhenSendingAndWaitingForReplies();
        }

        [Theory]
        [InlineData(1000, 10)]
        public void RepliesToConcurrentQueuedMessagesAreReceivedWithinAllottedTime(int count, int timeoutInSeconds)
        {
            GivenMessageCount(count);
            GivenTimeout(TimeSpan.FromSeconds(timeoutInSeconds));
            GivenQueuedOutboundDelivery();
            WhenSendingAndWaitingForReplies();
        }

        protected virtual void GivenMessageCount(int count)
        {
            Count = count;
        }

        protected virtual void GivenTimeout(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        protected virtual void GivenQueuedOutboundDelivery()
        {
            SendOptions = new SendOptions
            {
                Synchronous = false
            };
        }

        private void WhenSendingAndWaitingForReplies()
        {
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var sends = Enumerable.Range(0, Count)
                .Select(i => SendMessageAndAwaitReply(i, ct, SendOptions));

            var sw = Stopwatch.StartNew();
            cts.CancelAfter(Timeout);
            var sendResults = Task.WhenAll(sends).Result;
            sw.Stop();

            foreach (var sendResult in sendResults)
            {
                Assert.NotNull(sendResult.Reply);
                Assert.IsType<TestReply>(sendResult.Reply);

                var testReply = (TestReply)sendResult.Reply;
                Assert.Equal(sendResult.Message.GuidData, testReply.GuidData);
                Assert.Equal(sendResult.Message.IntData, testReply.IntData);
            }

            Output.WriteLine("Completed in {0}", sw.Elapsed);
        }

        protected virtual async Task<SendResult> SendMessageAndAwaitReply(int i, CancellationToken ct, SendOptions options = null)
        {
            var message = new TestMessage
            {
                DateData = DateTime.UtcNow,
                GuidData = Guid.NewGuid(),
                IntData = i,
                StringData = "TestMessage_" + i
            };

            try
            {
                var sentMessage = await Sender.Send(message, options, ct);
                var reply = await sentMessage.GetReply(ct);
                return new SendResult(message, reply, null);
            }
            catch (Exception ex)
            {
                return new SendResult(message, null, ex);
            }
        }

        protected class SendResult
        {
            public TestMessage Message { get; }
            public object Reply { get; }
            public Exception Exception { get; }

            public SendResult(TestMessage message, object reply, Exception exception)
            {
                Message = message;
                Reply = reply;
                Exception = exception;
            }
        }
    }
}