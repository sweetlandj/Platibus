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

                var expectation = new MessageHandledExpectation<TestPublication>((content, context) => publication.Equals(content));
                TestPublicationHandler.SetExpectation(expectation);

                await platibus0.Publish(publication, "Topic0");

                var timeout = Task.Delay(TimeSpan.FromSeconds(3));
                var timedOut = await Task.WhenAny(expectation.Satisfied, timeout) == timeout;
                Assert.False(timedOut);
                Assert.True(expectation.WasSatisfied);
            });
        }

        [Test]
        public async Task Given_2Subscribers_When_Message_Published_Then_Both_Subscribers_Should_Receive_It()
        {
            await With.OwinSelfHostedBusInstances(async (platibus0, platibus1) =>
            {
                // Start second subscriber.
                // Use HTTP server to verify hybrid hosting strategy.
                using (var httpServer2 = await HttpServer.Start("platibus.http2"))
                {
                    var platibus2 = httpServer2.Bus;
                    var publication = new TestPublication
                    {
                        GuidData = Guid.NewGuid(),
                        IntData = RNG.Next(0, int.MaxValue),
                        StringData = "Hello, world!",
                        DateData = DateTime.UtcNow
                    };

                    // Wait for subscription request for platibus.http2 to be processed
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    var expectation1 = new MessageHandledExpectation<TestPublication>((content, ctx) =>
                        publication.Equals(content) && ctx.Bus == platibus1);

                    var expectation2 = new MessageHandledExpectation<TestPublication>((content, ctx) =>
                        publication.Equals(content) && ctx.Bus == platibus2);

                    TestPublicationHandler.SetExpectation(expectation1);
                    TestPublicationHandler.SetExpectation(expectation2);

                    await platibus0.Publish(publication, "Topic0");

                    var expectationsSatisfied = Task.WhenAll(expectation1.Satisfied, expectation2.Satisfied);
                    var timeout = Task.Delay(TimeSpan.FromSeconds(3));
                    var timedOut = await Task.WhenAny(expectationsSatisfied, timeout) == timeout;
                    Assert.False(timedOut);
                    Assert.True(expectation1.WasSatisfied);
                    Assert.True(expectation2.WasSatisfied);
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

                var expectation = new MessageHandledExpectation<TestPublication>((content, ctx) =>
                    publication.Equals(content) && ctx.Bus == platibus1);

                TestPublicationHandler.SetExpectation(expectation);

                try
                {
                    await platibus0.Publish(publication, "Topic0");
                }
                catch (Exception)
                {
                    // We expect an aggregate exception with a 404 error for the second subscriber
                }

                var timeout = Task.Delay(TimeSpan.FromSeconds(3));
                var timedOut = await Task.WhenAny(expectation.Satisfied, timeout) == timeout;
                Assert.False(timedOut);
                Assert.True(expectation.WasSatisfied);
            });
        }
    }
}