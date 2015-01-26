using Common.Logging;
using NUnit.Framework;
using Pluribus.Filesystem;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pluribus.UnitTests
{
    class FilesystemSubscriptionTrackingServiceTests
    {
        private static readonly ILog Log = LogManager.GetLogger(UnitTestLoggingCategories.UnitTests);

        protected DirectoryInfo GetTempDirectory()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "Pluribus.UnitTests", DateTime.Now.ToString("yyyyMMddHHmmss"));
            var tempDir = new DirectoryInfo(tempPath);
            if (!tempDir.Exists)
            {
                tempDir.Create();
            }
            return tempDir;
        }

        [Test]
        public async Task Given_Many_Concurrent_Calls_When_Subscribing_Then_All_Subscriptions_Recorded_No_Errors()
        {
            var tempDir = GetTempDirectory();
            Log.DebugFormat("Temp directory: {0}", tempDir);

            var fsSubscriptionService = new FilesystemSubscriptionTrackingService(tempDir);
            await fsSubscriptionService.Init();

            var topicNames = Enumerable.Range(0, 10)
                .Select(i => new TopicName("Topic-" + i));

            var subscriptions = topicNames
                .SelectMany(t => 
                    Enumerable.Range(0, 100)
                        .Select(j => new Uri("http://localhost/subscriber-" + j))
                        .Select(s => new 
                        {
                            Topic = t,
                            Subscriber = s
                        }))
                        .ToList();

            await Task.WhenAll(subscriptions.Select(s => fsSubscriptionService.AddSubscription(s.Topic, s.Subscriber)));

            var subscriptionsByTopic = subscriptions.GroupBy(s => s.Topic);
            foreach(var grouping in subscriptionsByTopic)
            {
                var expectedSubscribers = grouping.Select(g => g.Subscriber).ToList();
                var actualSubscribers = fsSubscriptionService.GetSubscribers(grouping.Key);
                Assert.That(actualSubscribers, Is.EquivalentTo(expectedSubscribers));
            }
        }

        [Test]
        public async Task Given_Existing_Subscriptions_When_Initializing_Then_Subscriptions_Should_Be_Loaded()
        {
            var tempDir = GetTempDirectory();
            Log.DebugFormat("Temp directory: {0}", tempDir);

            var fsSubscriptionService = new FilesystemSubscriptionTrackingService(tempDir);
            await fsSubscriptionService.Init();

            var topicNames = Enumerable.Range(0, 10)
                .Select(i => new TopicName("Topic-" + i));

            var subscriptions = topicNames
                .SelectMany(t =>
                    Enumerable.Range(0, 100)
                        .Select(j => new Uri("http://localhost/subscriber-" + j))
                        .Select(s => new
                        {
                            Topic = t,
                            Subscriber = s
                        }))
                        .ToList();

            await Task.WhenAll(subscriptions.Select(s => fsSubscriptionService.AddSubscription(s.Topic, s.Subscriber)));

            // Next, initialize a new FilesystemSubscriptionService instance
            // with the same directory and observe that the subscriptions
            // are returned as expected.

            var fsSubscriptionService2 = new FilesystemSubscriptionTrackingService(tempDir);
            await fsSubscriptionService2.Init();

            var subscriptionsByTopic = subscriptions.GroupBy(s => s.Topic);
            foreach (var grouping in subscriptionsByTopic)
            {
                var expectedSubscribers = grouping.Select(g => g.Subscriber).ToList();
                var actualSubscribers = fsSubscriptionService2.GetSubscribers(grouping.Key);
                Assert.That(actualSubscribers, Is.EquivalentTo(expectedSubscribers));
            }
        }
    }
}
