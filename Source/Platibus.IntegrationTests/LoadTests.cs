// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
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
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Platibus.IntegrationTests
{
    class LoadTests
    {
        [Test]
        public async Task When_Sending_10_Test_Messages_10_Replies_Should_Be_Handled_Within_1s()
        {
            var elapsed = await RunTest(10);
            Assert.That(elapsed, Is.LessThan(TimeSpan.FromSeconds(10)));
        }

        [Test]
        public async Task When_Sending_100_Test_Messages_100_Replies_Should_Be_Handled_Within_5s()
        {
            var elapsed = await RunTest(100);
            Assert.That(elapsed, Is.LessThan(TimeSpan.FromSeconds(5)));
        }

        [Test]
        [Explicit]
        public async Task When_Sending_1000_Test_Messages_1000_Replies_Should_Be_Handled_Within_10s()
        {
            var elapsed = await RunTest(1000);
            Assert.That(elapsed, Is.LessThan(TimeSpan.FromSeconds(10)));
        }

        [Test]
        [Explicit]
        public async Task When_Sending_1100_Test_Messages_1100_Replies_Should_Be_Handled_Within_11s()
        {
            var elapsed = await RunTest(1100);
            Assert.That(elapsed, Is.LessThan(TimeSpan.FromSeconds(11)));
        }

        [Test]
        [Explicit]
        public async Task When_Sending_1500_Test_Messages_1500_Replies_Should_Be_Handled_Within_15s()
        {
            var elapsed = await RunTest(1500);
            Assert.That(elapsed, Is.LessThan(TimeSpan.FromSeconds(15)));
        }

        [Test]
        [Explicit]
        public async Task When_Sending_2000_Test_Messages_2000_Replies_Should_Be_Handled_Within_20s()
        {
            var elapsed = await RunTest(2000);
            Assert.That(elapsed, Is.LessThan(TimeSpan.FromSeconds(20)));
        }

        private async Task<TimeSpan> RunTest(int messageCount, bool durable = false)
        {
            return await With.HttpHostedBusInstances(async (platibus0, platibus1) =>
            {
                var sw = Stopwatch.StartNew();
                var sendOptions = new SendOptions { UseDurableTransport = durable };
                var repliesReceieved = Enumerable.Range(0, messageCount)
                    .Select(async i =>
                    {
                        var message = new TestMessage { IntData = i };
                        var sentMessage = await platibus0.Send(message, sendOptions);
                        return await sentMessage.GetReply();
                    });

                var replies = await Task.WhenAll(repliesReceieved);
                sw.Stop();
                Console.WriteLine("{0} messages sent and replies received in {1}", messageCount, sw.Elapsed);
                return sw.Elapsed;
            });
        }
    }
}
