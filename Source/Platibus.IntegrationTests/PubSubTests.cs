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
using System.Threading.Tasks;
using Xunit;

namespace Platibus.IntegrationTests
{
    public abstract class PubSubTests : IDisposable
    {
        private static readonly Random RNG = new Random();

        protected readonly Task<IBus> Publisher;
        protected readonly Task<IBus> Subscriber;

        protected TopicName Topic = "Topic0";
        protected TestPublication Publication;
        protected MessageHandledExpectation SubscriberHandlesPublication;
        private bool _disposed;

        protected PubSubTests(Task<IBus> publisher, Task<IBus> subscriber)
        {
            Publisher = publisher;
            Subscriber = subscriber;
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
            TestPublicationHandler.RemoveExpectation(SubscriberHandlesPublication);
        }
        
        [Fact]
        public async Task SubscriberReceivesPublications()
        {
            GivenTestPublication();
            await GivenSubscriberExpectsPublication();
            await WhenPublished();
            await AssertSubscriberReceivesAndHandlesThePublication();
        }

        [Fact]
        public async Task SubscriptionsAutomaticallyRenewBeforeTheyExpire()
        {
            GivenTestPublication();
            await GivenSubscriberExpectsPublication();

            // Subscriptions have a 10 second TTL declaratively configured in app.config
            var subscriptionTtl = TimeSpan.FromSeconds(10);
            await WhenPublishedAfterTTLElapses(subscriptionTtl);
            await AssertSubscriberReceivesAndHandlesThePublication();
        }
        
        protected void GivenTestPublication()
        {
            Publication = new TestPublication
            {
                GuidData = Guid.NewGuid(),
                IntData = RNG.Next(0, int.MaxValue),
                StringData = "Hello, world!",
                DateData = DateTime.UtcNow
            };    
        }

        protected async Task GivenSubscriberExpectsPublication()
        {
            var subscriber = await Subscriber;
            SubscriberHandlesPublication = new MessageHandledExpectation<TestPublication>((content, ctx) =>
                Publication.Equals(content) && ctx.Bus == subscriber);

            TestPublicationHandler.SetExpectation(SubscriberHandlesPublication);

            // Wait briefly to ensure subscriptions propagate
            await Task.Delay(TimeSpan.FromMilliseconds(250));
        }

        protected async Task WhenPublished()
        {
            var publisher = await Publisher;
            await publisher.Publish(Publication, Topic);
        }

        protected async Task WhenPublishedAfterTTLElapses(TimeSpan ttl)
        {
            // Wait for TTL to expire
            var buffer = TimeSpan.FromMilliseconds(500);
            await Task.Delay(ttl.Add(buffer));

            await WhenPublished();
        }

        protected async Task AssertSubscriberReceivesAndHandlesThePublication()
        {
            var timeout = Task.Delay(TimeSpan.FromMinutes(5));
            var timedOut = await Task.WhenAny(SubscriberHandlesPublication.Satisfied, timeout) == timeout;
            Assert.False(timedOut);
            Assert.True(SubscriberHandlesPublication.WasSatisfied);
        }
    }
}