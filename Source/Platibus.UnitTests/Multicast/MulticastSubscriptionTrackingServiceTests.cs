using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Platibus.Multicast;
using Platibus.UnitTests.Stubs;

namespace Platibus.UnitTests.Multicast
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class MulticastSubscriptionTrackingServiceTests : IDisposable
    {
        protected SubscriptionTrackingServiceStub SendingSubscriptionTrackingServiceStub;
        protected MulticastSubscriptionTrackingService SendingMulticastSubscriptionTrackingService;
        protected SubscriptionTrackingServiceStub ReceivingSubscriptionTrackingServiceStub;
        protected MulticastSubscriptionTrackingService ReceivingMulticastSubscriptionTrackingService;

        public MulticastSubscriptionTrackingServiceTests()
        {
            var groupAddress = IPAddress.Parse("239.255.21.80");
            const int port = 52181;

            SendingSubscriptionTrackingServiceStub = new SubscriptionTrackingServiceStub();
            SendingMulticastSubscriptionTrackingService = new MulticastSubscriptionTrackingService(
                SendingSubscriptionTrackingServiceStub, groupAddress, port);

            ReceivingSubscriptionTrackingServiceStub = new SubscriptionTrackingServiceStub();
            ReceivingMulticastSubscriptionTrackingService = new MulticastSubscriptionTrackingService(
                ReceivingSubscriptionTrackingServiceStub, groupAddress, port);
        }

        public void Dispose()
        {
            SendingMulticastSubscriptionTrackingService.TryDispose();
            ReceivingMulticastSubscriptionTrackingService.TryDispose();
        }

        [Fact]
        public async Task AdditionsShouldPropagateToGroupMembers()
        {
            var topic = new TopicName("TestTopic");
            var subscriber = new Uri("http://localhost/platibus");
            var ttl = TimeSpan.FromSeconds(720);

            var broadcastReceived = new TaskCompletionSource<bool>();
            ReceivingSubscriptionTrackingServiceStub.SubscriptionAdded +=
                (source, args) =>
                {
                    broadcastReceived.TrySetResult(true);
                };

            await SendingMulticastSubscriptionTrackingService.AddSubscription(topic, subscriber, ttl);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            cts.Token.Register(() => broadcastReceived.TrySetCanceled());
            await broadcastReceived.Task;

            var subscribers = await ReceivingSubscriptionTrackingServiceStub.GetSubscribers(topic, cts.Token);
            Assert.Contains(subscriber, subscribers);
        }

        [Fact]
        public async Task RemovalsShouldPropagateToGroupMembers()
        {
            var topic = new TopicName("TestTopic");
            var subscriber = new Uri("http://localhost/platibus");

            var broadcastReceived = new TaskCompletionSource<bool>();
            ReceivingSubscriptionTrackingServiceStub.SubscriptionRemoved +=
                (source, args) =>
                {
                    broadcastReceived.TrySetResult(true);
                };

            await SendingMulticastSubscriptionTrackingService.RemoveSubscription(topic, subscriber);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            cts.Token.Register(() => broadcastReceived.TrySetCanceled());
            await broadcastReceived.Task;

            var subscribers = await ReceivingSubscriptionTrackingServiceStub.GetSubscribers(topic, cts.Token);
            Assert.DoesNotContain(subscriber, subscribers);
        }
    }
}
