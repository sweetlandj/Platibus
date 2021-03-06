﻿// The MIT License (MIT)
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
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Platibus.Config;

namespace Platibus.UnitTests
{
    [Trait("Category", "UnitTests")]
    public class BusInitTests
    {
        protected Uri BaseUri = new Uri("http://localhost:52180/platibus/");
        protected PlatibusConfiguration Configuration;
        protected IMessageQueueingService MessageQueueingService;
        protected ITransportService TransportService;

        [Fact]
        public async Task OverriddenQueueOptionsShouldTakePrecedenceOverDefaults()
        {
            GivenDefaultConfiguration();
            GivenMockTransportService();
            var mockQueueingService = GivenMockQueueingService();

            var queueName = new QueueName("queue1");
            var handlingRule1 = FakeHandlingRule(".*", queueName, null);
            Configuration.AddHandlingRule(handlingRule1);

            var overriddenOptions = new QueueOptions
            {
                ConcurrencyLimit = 1,
                MaxAttempts = 5,
                RetryDelay = TimeSpan.FromMinutes(15)
            };
            var handlingRule2 = FakeHandlingRule(".*", queueName, overriddenOptions);
            Configuration.AddHandlingRule(handlingRule2);

            await WhenInitializingBus();

            mockQueueingService.Verify(
                x => x.CreateQueue(queueName, It.IsAny<IQueueListener>(), overriddenOptions,
                    It.IsAny<CancellationToken>()), Times.Once());
        }

        private void GivenDefaultConfiguration()
        {
            Configuration = new PlatibusConfiguration();
        }

        private Mock<IMessageQueueingService> GivenMockQueueingService()
        {
            var mockQueueingService = new Mock<IMessageQueueingService>();
            mockQueueingService.Setup(x => x.CreateQueue(
                   It.IsAny<QueueName>(),
                   It.IsAny<IQueueListener>(),
                   It.IsAny<QueueOptions>(),
                   It.IsAny<CancellationToken>()))
               .Returns(Task.FromResult(true));

            MessageQueueingService = mockQueueingService.Object;
            return mockQueueingService;
        }

        private void GivenMockTransportService()
        {
            var mockTransportService = new Mock<ITransportService>();
            mockTransportService.Setup(x => x.SendMessage(
                    It.IsAny<Message>(),
                    It.IsAny<IEndpointCredentials>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            mockTransportService.Setup(x => x.PublishMessage(
                    It.IsAny<Message>(),
                    It.IsAny<TopicName>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            mockTransportService.Setup(x => x.Subscribe(
                    It.IsAny<IEndpoint>(),
                    It.IsAny<TopicName>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            TransportService = mockTransportService.Object;
        }

        private async Task WhenInitializingBus()
        {
            var bus = new Bus(Configuration, BaseUri, TransportService, MessageQueueingService);
            await bus.Init();
        }

        private static IHandlingRule FakeHandlingRule(string pattern, QueueName queueName, QueueOptions queueOptions)
        {
            var messageSpec = new MessageNamePatternSpecification(pattern);
            var messageHandler = new DelegateMessageHandler((o, ctx) => { });
            return new HandlingRule(messageSpec, messageHandler, queueName, queueOptions);
        }
    }
}
