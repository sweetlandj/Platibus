using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Platibus.IntegrationTests
{
    internal class RabbitMQPubSubTests
    {
        private static readonly Random RNG = new Random();

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

                await platibus0.Publish(publication, "Topic0");

                var publicationReceived = await TestPublicationHandler.WaitHandle.WaitOneAsync(TimeSpan.FromSeconds(30));
                Assert.That(publicationReceived, Is.True);
            });
        }
    }
}