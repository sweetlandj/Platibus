// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Platibus.Multicast;
using Platibus.UnitTests.Stubs;

namespace Platibus.UnitTests.Multicast
{
    [Trait("Category", "UnitTests")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class MulticastSubscriptionTrackingServiceTests : IDisposable
    {
        protected SubscriptionTrackingServiceStub SendingSubscriptionTrackingServiceStub;
        protected MulticastSubscriptionTrackingService SendingMulticastSubscriptionTrackingService;
        protected SubscriptionTrackingServiceStub ReceivingSubscriptionTrackingServiceStub;
        protected MulticastSubscriptionTrackingService ReceivingMulticastSubscriptionTrackingService;

        private bool _disposed;

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

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                SendingMulticastSubscriptionTrackingService.Dispose();
                ReceivingMulticastSubscriptionTrackingService.Dispose();
            }
        }
    }
}
