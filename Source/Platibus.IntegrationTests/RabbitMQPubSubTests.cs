using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Platibus.IntegrationTests
{
    internal class RabbitMQPubSubTests
    {
        private static readonly Random RNG = new Random();

        [SetUp]
        public void SetUp()
        {
            TestPublicationHandler.Reset();
        }

        [Explicit]
        [Test]
        public async Task Given_Subscriber_When_Message_Published_Then_Subscriber_Should_Receive_It()
        {
            await With.RabbitMQHostedBusInstances(async (platibus0, platibus1) =>
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
    }
}