using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Platibus.IntegrationTests
{
    internal class RabbitMQSendAndReplyTests
    {
        private static readonly Random RNG = new Random();

        [Explicit]
        [Test]
        public async Task Given_Message_When_Sending_Then_Reply_Should_Be_Observed()
        {
            await With.RabbitMQHostedBusInstances(async (platibus0, platibus1) =>
            {
                var replyReceivedEvent = new ManualResetEvent(false);
                var repliesCompletedEvent = new ManualResetEvent(false);
                var replies = new ConcurrentQueue<object>();
                var message = new TestMessage
                {
                    GuidData = Guid.NewGuid(),
                    IntData = RNG.Next(0, int.MaxValue),
                    StringData = "Hello, world!",
                    DateData = DateTime.UtcNow
                };

                var sentMessage = await platibus0.Send(message);
                var subscription = sentMessage
                    .ObserveReplies()
                    .Subscribe(r =>
                    {
                        replies.Enqueue(r);
                        replyReceivedEvent.Set();
                    }, () => repliesCompletedEvent.Set());

                var replyReceived = await replyReceivedEvent.WaitOneAsync(TimeSpan.FromSeconds(30));
                var repliesCompleted = await repliesCompletedEvent.WaitOneAsync(TimeSpan.FromSeconds(30));
                subscription.Dispose();

                Assert.That(replyReceived, Is.True);
                Assert.That(repliesCompleted, Is.True);
                Assert.That(replies.Count, Is.EqualTo(1));

                var reply = replies.First();
                Assert.That(reply, Is.InstanceOf<TestReply>());
            });
        }
    }
}