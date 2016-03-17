// The MIT License (MIT)
// 
// Copyright (c) 2015 Jesse Sweetland
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
using NUnit.Framework;
using Platibus.Http;

namespace Platibus.IntegrationTests
{
    internal class HttpPubSubTests
    {
        private static readonly Random RNG = new Random();

        [SetUp]
        public void SetUp()
        {
            TestPublicationHandler.Reset();
        }

        [Test]
        public async Task Given_Subscriber_When_Message_Published_Then_Subscriber_Should_Receive_It()
        {
            await With.HttpHostedBusInstances(async (platibus0, platibus1) =>
            {
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

        [Test]
        public async Task Given_2Subscribers_When_Message_Published_Then_Both_Subscribers_Should_Receive_It()
        {
            await With.HttpHostedBusInstances(async (platibus0, platibus1) =>
            {
                // Start second subscriber
                using (await HttpServer.Start("platibus.http2"))
                {
                    // Wait for two publications to be received (one on each subscriber)
                    TestPublicationHandler.MessageReceivedEvent.AddCount(1);

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
                }
            });
        }

        [Test]
        public async Task Given_2Subscribers_1_Not_Available_When_Message_Published_Then_The_Available_Subscribers_Should_Receive_It()
        {
            await With.HttpHostedBusInstances(async (platibus0, platibus1) =>
            {
                // Start second subscriber to create the subscription, then immediately stop it
                using (await HttpServer.Start("platibus.http2"))
                {
                }

                var publication = new TestPublication
                {
                    GuidData = Guid.NewGuid(),
                    IntData = RNG.Next(0, int.MaxValue),
                    StringData = "Hello, world!",
                    DateData = DateTime.UtcNow
                };

                try
                {
                    await platibus0.Publish(publication, "Topic0");
                }
                catch (Exception)
                {
                    // We expect an aggregate exception with a 404 error for the second subscriber
                }

                var publicationReceived = await TestPublicationHandler.WaitHandle.WaitOneAsync(TimeSpan.FromSeconds(3));
                Assert.That(publicationReceived, Is.True);
            });
        }
    }
}