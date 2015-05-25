using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using NUnit.Framework;
using Platibus.SQLite;

namespace Platibus.UnitTests
{
    internal class SQLiteSubscriptionTrackingServiceTests
    {
        private static readonly ILog Log = LogManager.GetLogger(UnitTestLoggingCategories.UnitTests);

        protected DirectoryInfo GetTempDirectory()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "Platibus.UnitTests",
                DateTime.Now.ToString("yyyyMMddHHmmss"));
            var tempDir = new DirectoryInfo(tempPath);
            if (!tempDir.Exists)
            {
                tempDir.Create();
            }
            return tempDir;
        }

        [Test]
        public async Task Handles_Many_Concurrent_Calls_Correctly_Without_Errors()
        {
            var tempDir = GetTempDirectory();
            Log.DebugFormat("Temp directory: {0}", tempDir);

            var sqliteSubscriptionService = new SQLiteSubscriptionTrackingService(tempDir);
            await sqliteSubscriptionService.Init();

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

            await
                Task.WhenAll(subscriptions.Select(s => sqliteSubscriptionService.AddSubscription(s.Topic, s.Subscriber)));

            var subscriptionsByTopic = subscriptions.GroupBy(s => s.Topic);
            foreach (var grouping in subscriptionsByTopic)
            {
                var expectedSubscribers = grouping.Select(g => g.Subscriber).ToList();
                var actualSubscribers = await sqliteSubscriptionService.GetSubscribers(grouping.Key);
                Assert.That(actualSubscribers, Is.EquivalentTo(expectedSubscribers));
            }
        }

        [Test]
        public async Task Subscriber_Should_Not_Be_Returned_After_It_Is_Removed()
        {
            var tempDir = GetTempDirectory();
            Log.DebugFormat("Temp directory: {0}", tempDir);

            var sqliteSubscriptionService = new SQLiteSubscriptionTrackingService(tempDir);
            await sqliteSubscriptionService.Init();

            var topic = "topic-0";
            var subscriber = new Uri("http://localhost/platibus");
            await sqliteSubscriptionService.AddSubscription(topic, subscriber);

            var subscribers = await sqliteSubscriptionService.GetSubscribers(topic);
            Assert.That(subscribers, Has.Member(subscriber));

            await sqliteSubscriptionService.RemoveSubscription(topic, subscriber);

            var subscribersAfterRemoval = await sqliteSubscriptionService.GetSubscribers(topic);
            Assert.That(subscribersAfterRemoval, Has.No.Member(subscriber));
        }

        [Test]
        public async Task Expired_Subscription_Should_Not_Be_Returned()
        {
            var tempDir = GetTempDirectory();
            Log.DebugFormat("Temp directory: {0}", tempDir);

            var sqliteSubscriptionService = new SQLiteSubscriptionTrackingService(tempDir);
            await sqliteSubscriptionService.Init();

            var topic = "topic-0";
            var subscriber = new Uri("http://localhost/platibus");
            await sqliteSubscriptionService.AddSubscription(topic, subscriber, TimeSpan.FromMilliseconds(1));

            await Task.Delay(TimeSpan.FromMilliseconds(100));

            var subscribers = await sqliteSubscriptionService.GetSubscribers(topic);
            Assert.That(subscribers, Has.No.Member(subscriber));
        }

        [Test]
        public async Task Expired_Subscription_That_Is_Renewed_Should_Be_Returned()
        {
            var tempDir = GetTempDirectory();
            Log.DebugFormat("Temp directory: {0}", tempDir);

            var sqliteSubscriptionService = new SQLiteSubscriptionTrackingService(tempDir);
            await sqliteSubscriptionService.Init();

            var topic = "topic-0";
            var subscriber = new Uri("http://localhost/platibus");
            await sqliteSubscriptionService.AddSubscription(topic, subscriber, TimeSpan.FromMilliseconds(1));

            await Task.Delay(TimeSpan.FromMilliseconds(100));

            var subscribers = await sqliteSubscriptionService.GetSubscribers(topic);
            Assert.That(subscribers, Has.No.Member(subscriber));

            await sqliteSubscriptionService.AddSubscription(topic, subscriber, TimeSpan.FromSeconds(5));

            var subscribersAfterRenewal = await sqliteSubscriptionService.GetSubscribers(topic);
            Assert.That(subscribersAfterRenewal, Has.Member(subscriber));
        }

        [Test]
        public async Task Existing_Subscriptions_Should_Be_Loaded_When_Initializing()
        {
            var tempDir = GetTempDirectory();
            Log.DebugFormat("Temp directory: {0}", tempDir);

            var sqliteSubscriptionService = new SQLiteSubscriptionTrackingService(tempDir);
            await sqliteSubscriptionService.Init();

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

            await
                Task.WhenAll(subscriptions.Select(s => sqliteSubscriptionService.AddSubscription(s.Topic, s.Subscriber)));

            // Next, initialize a new SQLiteSubscriptionService instance
            // with the same directory and observe that the subscriptions
            // are returned as expected.

            var sqliteSubscriptionService2 = new SQLiteSubscriptionTrackingService(tempDir);
            await sqliteSubscriptionService2.Init();

            var subscriptionsByTopic = subscriptions.GroupBy(s => s.Topic);
            foreach (var grouping in subscriptionsByTopic)
            {
                var expectedSubscribers = grouping.Select(g => g.Subscriber).ToList();
                var actualSubscribers = await sqliteSubscriptionService2.GetSubscribers(grouping.Key);
                Assert.That(actualSubscribers, Is.EquivalentTo(expectedSubscribers));
            }
        }
    }
}