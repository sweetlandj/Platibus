using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using NUnit.Framework;
using Platibus.SQL;

namespace Platibus.UnitTests
{
    internal class SQLSubscriptionTrackingServiceTests
    {
        private static readonly ILog Log = LogManager.GetLogger(UnitTestLoggingCategories.UnitTests);

        protected ConnectionStringSettings GetConnectionStringSettings()
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["PlatibusUnitTests.LocalDB"];
            using (var connection = connectionStringSettings.OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    IF (OBJECT_ID('[PB_Subscriptions]')) IS NOT NULL 
                    BEGIN
                        DELETE FROM [PB_Subscriptions]
                    END";

                command.ExecuteNonQuery();
            }
            return connectionStringSettings;
        }

        [Test]
        public async Task Handles_Many_Concurrent_Calls_Correctly_Without_Errors()
        {
            var connectionStringSettings = GetConnectionStringSettings();
            var sqlSubscriptionService = new SQLSubscriptionTrackingService(connectionStringSettings);
            await sqlSubscriptionService.Init();

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

            await Task.WhenAll(subscriptions.Select(s => sqlSubscriptionService.AddSubscription(s.Topic, s.Subscriber)));

            var subscriptionsByTopic = subscriptions.GroupBy(s => s.Topic);
            foreach (var grouping in subscriptionsByTopic)
            {
                var expectedSubscribers = grouping.Select(g => g.Subscriber).ToList();
                var actualSubscribers = await sqlSubscriptionService.GetSubscribers(grouping.Key);
                Assert.That(actualSubscribers, Is.EquivalentTo(expectedSubscribers));
            }
        }

        [Test]
        public async Task Subscriber_Should_Not_Be_Returned_After_It_Is_Removed()
        {
            var connectionStringSettings = GetConnectionStringSettings();
            var sqlSubscriptionService = new SQLSubscriptionTrackingService(connectionStringSettings);
            await sqlSubscriptionService.Init();

            var topic = "topic-0";
            var subscriber = new Uri("http://localhost/platibus");
            await sqlSubscriptionService.AddSubscription(topic, subscriber);

            var subscribers = await sqlSubscriptionService.GetSubscribers(topic);
            Assert.That(subscribers, Has.Member(subscriber));

            await sqlSubscriptionService.RemoveSubscription(topic, subscriber);

            var subscribersAfterRemoval = await sqlSubscriptionService.GetSubscribers(topic);
            Assert.That(subscribersAfterRemoval, Has.No.Member(subscriber));
        }

        [Test]
        public async Task Expired_Subscription_Should_Not_Be_Returned()
        {
            var connectionStringSettings = GetConnectionStringSettings();
            Log.DebugFormat("Temp directory: {0}", connectionStringSettings);

            var sqlSubscriptionService = new SQLSubscriptionTrackingService(connectionStringSettings);
            await sqlSubscriptionService.Init();

            var topic = "topic-0";
            var subscriber = new Uri("http://localhost/platibus");
            await sqlSubscriptionService.AddSubscription(topic, subscriber, TimeSpan.FromMilliseconds(1));

            await Task.Delay(TimeSpan.FromMilliseconds(100));

            var subscribers = await sqlSubscriptionService.GetSubscribers(topic);
            Assert.That(subscribers, Has.No.Member(subscriber));
        }

        [Test]
        public async Task Expired_Subscription_That_Is_Renewed_Should_Be_Returned()
        {
            var connectionStringSettings = GetConnectionStringSettings();
            var sqlSubscriptionService = new SQLSubscriptionTrackingService(connectionStringSettings);
            await sqlSubscriptionService.Init();

            var topic = "topic-0";
            var subscriber = new Uri("http://localhost/platibus");
            await sqlSubscriptionService.AddSubscription(topic, subscriber, TimeSpan.FromMilliseconds(1));

            await Task.Delay(TimeSpan.FromMilliseconds(100));

            var subscribers = await sqlSubscriptionService.GetSubscribers(topic);
            Assert.That(subscribers, Has.No.Member(subscriber));

            await sqlSubscriptionService.AddSubscription(topic, subscriber, TimeSpan.FromSeconds(5));

            var subscribersAfterRenewal = await sqlSubscriptionService.GetSubscribers(topic);
            Assert.That(subscribersAfterRenewal, Has.Member(subscriber));
        }

        [Test]
        public async Task Existing_Subscriptions_Should_Be_Loaded_When_Initializing()
        {
            var connectionStringSettings = GetConnectionStringSettings();
            var sqlSubscriptionService = new SQLSubscriptionTrackingService(connectionStringSettings);
            await sqlSubscriptionService.Init();

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

            await Task.WhenAll(subscriptions.Select(s => sqlSubscriptionService.AddSubscription(s.Topic, s.Subscriber)));

            // Next, initialize a new SQLSubscriptionService instance
            // with the same directory and observe that the subscriptions
            // are returned as expected.

            var sqlSubscriptionService2 = new SQLSubscriptionTrackingService(connectionStringSettings);
            await sqlSubscriptionService2.Init();

            var subscriptionsByTopic = subscriptions.GroupBy(s => s.Topic);
            foreach (var grouping in subscriptionsByTopic)
            {
                var expectedSubscribers = grouping.Select(g => g.Subscriber).ToList();
                var actualSubscribers = await sqlSubscriptionService2.GetSubscribers(grouping.Key);
                Assert.That(actualSubscribers, Is.EquivalentTo(expectedSubscribers));
            }
        }
    }
}