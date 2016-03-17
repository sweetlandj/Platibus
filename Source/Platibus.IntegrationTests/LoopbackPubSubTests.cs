using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Platibus.IntegrationTests
{
    internal class LoopbackPubSubTests
    {
        private static readonly Random RNG = new Random();

        [SetUp]
        public void SetUp()
        {
            TestPublicationHandler.Reset();
        }

        [Test]
        public async Task Given_Automatic_Subscription_When_Message_Published_Then_Subscriber_Should_Receive_It()
        {
            await With.LoopbackInstance(async bus =>
            {
                var publication = new TestPublication
                {
                    GuidData = Guid.NewGuid(),
                    IntData = RNG.Next(0, int.MaxValue),
                    StringData = "Hello, world!",
                    DateData = DateTime.UtcNow
                };

                await bus.Publish(publication, "Topic0");

                var publicationReceived = await TestPublicationHandler.WaitHandle.WaitOneAsync(TimeSpan.FromSeconds(3));
                Assert.That(publicationReceived, Is.True);
            });
        }
    }
}