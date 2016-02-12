
using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Platibus.IntegrationTests
{
	class HttpSubscriptionRenewalTests
	{
		private static readonly Random RNG = new Random();

		[Test]
		public async Task Given_10Second_TTL_When_Publishing_Message_After_15Seconds_The_Publication_Should_Be_Received()
		{
			await With.HttpHostedBusInstances(async (platibus0, platibus1) =>
			{
				await Task.Delay(TimeSpan.FromSeconds(15));

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
	}
}
