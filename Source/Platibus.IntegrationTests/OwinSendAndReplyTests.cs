using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Platibus.IntegrationTests
{
    internal class OwinSendAndReplyTests
    {
        private static readonly Random RNG = new Random();

        [Test]
        public async Task Given_Noncritical_Message_Not_Authorized_When_Sending_Then_UnauthorizedAccessException()
        {
            await With.OwinSelfHostedBusInstances(async (platibus0, platibus1) =>
            {
                var message = new TestMessage
                {
                    GuidData = Guid.NewGuid(),
                    IntData = RNG.Next(0, int.MaxValue),
                    StringData = "Hello, world!",
                    DateData = DateTime.UtcNow,
                    SimulateAuthorizationFailure = true
                };

                Exception exception = null;
                try
                {
                    await platibus0.Send(message, new SendOptions
                    {
                        Importance = MessageImportance.Low
                    });
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                Assert.That(exception, Is.Not.Null);
                Assert.That(exception, Is.InstanceOf<UnauthorizedAccessException>());
            });
        }

        [Test]
        public async Task Given_Noncritical_Message_Not_Acknowledged_When_Sending_Then_MessageNotAcknowledgedException()
        {
            await With.OwinSelfHostedBusInstances(async (platibus0, platibus1) =>
            {
                var message = new TestMessage
                {
                    GuidData = Guid.NewGuid(),
                    IntData = RNG.Next(0, int.MaxValue),
                    StringData = "Hello, world!",
                    DateData = DateTime.UtcNow,
                    SimulateAcknowledgementFailure = true
                };

                Exception exception = null;
                try
                {
                    await platibus0.Send(message, new SendOptions
                    {
                        Importance = MessageImportance.Low
                    });
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                Assert.That(exception, Is.Not.Null);
                Assert.That(exception, Is.InstanceOf<MessageNotAcknowledgedException>());
            });
        }

        [Test]
        public async Task Given_Message_When_Sending_Then_Reply_Should_Be_Observed()
        {
            await With.OwinSelfHostedBusInstances(async (platibus0, platibus1) =>
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
                subscription.TryDispose();

                Assert.That(replyReceived, Is.True);
                Assert.That(repliesCompleted, Is.True);
                Assert.That(replies.Count, Is.EqualTo(1));

                var reply = replies.First();
                Assert.That(reply, Is.InstanceOf<TestReply>());
            });
        }
    }
}