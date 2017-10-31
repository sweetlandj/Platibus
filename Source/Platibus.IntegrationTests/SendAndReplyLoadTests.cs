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
        protected readonly Task<IBus> Sender;
        protected int Count;
        protected TimeSpan Timeout;
        protected SendOptions SendOptions = new SendOptions {Synchronous = true};

        protected SendAndReplyLoadTests(Task<IBus> sender, ITestOutputHelper output)
        {
            Sender = sender;
            Output = output;
        }

        [Theory]
        [InlineData(1000, 3)]
        [InlineData(2000, 5)]
        [InlineData(5000, 15)]
        public void RepliesToConcurrentMessagesAreReceivedWithinAlottedTime(int count, int timeoutInSeconds)
        {
            GivenMessageCount(count);
            GivenTimeout(TimeSpan.FromSeconds(timeoutInSeconds));
            WhenSendingAndWaitingForReplies();
        }

        [Theory]
        [InlineData(1000, 5)]
        [InlineData(2000, 10)]
        [InlineData(5000, 20)]
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
            Task.WhenAll(sends).Wait(ct);
            sw.Stop();
            Output.WriteLine("Completed in {0}", sw.Elapsed);
        }

        protected virtual async Task SendMessageAndAwaitReply(int i, CancellationToken ct, SendOptions options = null)
        {
            try
            {
                var message = new TestMessage
                {
                    DateData = DateTime.UtcNow,
                    GuidData = Guid.NewGuid(),
                    IntData = i,
                    StringData = "TestMessage_" + i
                };

                var bus = await Sender;

                var sentMessage = await bus.Send(message, options, ct);
                var reply = await sentMessage.GetReply(ct);
                Assert.NotNull(reply);
                Assert.IsType(typeof(TestReply), reply);

                var testReply = (TestReply)reply;
                Assert.Equal(message.GuidData, testReply.GuidData);
                Assert.Equal(message.IntData, testReply.IntData);
            }
            catch (Exception e)
            {
                Output.WriteLine("Send/receive error: {0}", e);
                throw;
            }
        }
    }
}