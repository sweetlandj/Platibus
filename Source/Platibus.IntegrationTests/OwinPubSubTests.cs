using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Platibus.Http;

namespace Platibus.IntegrationTests
{
    internal class OwinPubSubTests
    {
        private static readonly Random RNG = new Random();

        [SetUp]
        public void SetUp()
        {
            TestPublicationHandler.Reset();
        }

        [Test]
        public async Task Given_Subscriber_When_Message_Published_Then_Subscriber_Should_Receive_It()
        {
            await With.OwinSelfHostedBusInstances(async (platibus0, platibus1) =>
            {
                var publication = new TestPublication
                {
                    GuidData = Guid.NewGuid(),
                    IntData = RNG.Next(0, int.MaxValue),
                    StringData = "Hello, world!",
                    DateData = DateTime.UtcNow
                };

                await platibus0.Publish(publication, "Topic0");

                var publicationReceived = await TestPublicationHandler.WaitHandle.WaitOneAsync(TimeSpan.FromSeconds(3));
                Assert.That(publicationReceived, Is.True);
            });
        }

        [Test]
        public async Task Given_2Subscribers_When_Message_Published_Then_Both_Subscribers_Should_Receive_It()
        {
            await With.OwinSelfHostedBusInstances(async (platibus0, platibus1) =>
            {
                // Start second subscriber.
                // Use HTTP server to verify hybrid hosting strategy.
                using (await HttpServer.Start("platibus.http2"))
                {
                    // Wait for two publications to be received (one on each subscriber)
                    TestPublicationHandler.MessageReceivedEvent.AddCount(1);

                    var publication = new TestPublication
                    {
                        GuidData = Guid.NewGuid(),
                        IntData = RNG.Next(0, int.MaxValue),
                        StringData = "Hello, world!",
                        DateData = DateTime.UtcNow
                    };

                    // Wait for subscription request for platibus.http2 to be processed
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    await platibus0.Publish(publication, "Topic0");

                    var publicationReceived = await TestPublicationHandler.WaitHandle.WaitOneAsync(TimeSpan.FromSeconds(3));
                    Assert.That(publicationReceived, Is.True);
                }
            });
        }

        [Test]
        public async Task Given_2Subscribers_1_Not_Available_When_Message_Published_Then_The_Available_Subscribers_Should_Receive_It()
        {
            await With.OwinSelfHostedBusInstances(async (platibus0, platibus1) =>
            {
                // Start second subscriber to create the subscription, then immediately stop it.
                // Use HTTP server to verify hybrid hosting strategy.
                using (await HttpServer.Start("platibus.http2"))
                {
                }

                var publication = new TestPublication
                {
                    GuidData = Guid.NewGuid(),
                    IntData = RNG.Next(0, int.MaxValue),
                    StringData = "Hello, world!",
                    DateData = DateTime.UtcNow
                };

                try
                {
                    await platibus0.Publish(publication, "Topic0");
                }
                catch (Exception)
                {
                    // We expect an aggregate exception with a 404 error for the second subscriber
                }

                var publicationReceived = await TestPublicationHandler.WaitHandle.WaitOneAsync(TimeSpan.FromSeconds(3));
                Assert.That(publicationReceived, Is.True);
            });
        }
    }
}