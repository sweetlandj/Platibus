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
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Platibus.Config;
using Platibus.Security;

namespace Platibus.UnitTests
{
    [Trait("Category", "UnitTests")]
    public class BusSendTests
    {
        protected PlatibusConfiguration Configuration = new PlatibusConfiguration
        {
            DefaultContentType = "text/plain"
        };

        protected readonly EndpointName DefaultEndpointName = "local";
        protected readonly Uri DefaultEndpointUri = new Uri("http://localhost/default/");
        protected readonly IEndpointCredentials DefaultEndpointCredentials = new BasicAuthCredentials("default", "defaultpw");

        protected readonly EndpointName OtherEndpointName = "other";
        protected readonly Uri OtherEndpointUri = new Uri("http://localhost/other/");
        protected readonly IEndpointCredentials OtherEndpointCredentials = new BasicAuthCredentials("other", "otherpw");

        [Fact]
        public async Task SendRulesDetermineMessageDestination()
        {
            var mockTransportService = GivenMockTransportService();
            const string messageContent = "Hello, world!";
            using (var bus = await GivenConfiguredBus(mockTransportService.Object))
            {
                await bus.Send(messageContent);
            }

            // Only one call to ITransportService.SendMessage
            mockTransportService.Verify(
                ts => ts.SendMessage(
                    It.IsAny<Message>(),
                    It.IsAny<IEndpointCredentials>(),
                    It.IsAny<CancellationToken>()),
                Times.Once());

            // Specifically, only one call to ITransportService.SendMessage for this message
            // addressed to the expected destination
            mockTransportService.Verify(
                ts => ts.SendMessage(
                    It.Is<Message>(m => m.Content == messageContent && m.Headers.Destination == DefaultEndpointUri),
                    DefaultEndpointCredentials,
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }

        [Fact]
        public async Task MessageCanBeAddressedToSpecificEndpointByName()
        {
            var mockTransportService = GivenMockTransportService();
            const string messageContent = "Hello, world!";
            using (var bus = await GivenConfiguredBus(mockTransportService.Object))
            {
                await bus.Send(messageContent, OtherEndpointName);
            }

            // Only one call to ITransportService.SendMessage
            mockTransportService.Verify(
                ts => ts.SendMessage(
                    It.IsAny<Message>(),
                    It.IsAny<IEndpointCredentials>(),
                    It.IsAny<CancellationToken>()),
                Times.Once());

            // Specifically, only one call to ITransportService.SendMessage for this message
            // addressed to the expected destination
            mockTransportService.Verify(
                ts => ts.SendMessage(
                    It.Is<Message>(m => m.Content == messageContent && m.Headers.Destination == OtherEndpointUri),
                    OtherEndpointCredentials,
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }

        [Fact]
        public async Task MessageCanBeAddressedToSpecificEndpointUri()
        {
            var mockTransportService = GivenMockTransportService();
            const string messageContent = "Hello, world!";
            using (var bus = await GivenConfiguredBus(mockTransportService.Object))
            {
                await bus.Send(messageContent, OtherEndpointUri);
            }

            // Only one call to ITransportService.SendMessage
            mockTransportService.Verify(
                ts => ts.SendMessage(
                    It.IsAny<Message>(),
                    It.IsAny<IEndpointCredentials>(),
                    It.IsAny<CancellationToken>()),
                Times.Once());

            // Specifically, only one call to ITransportService.SendMessage for this message
            // addressed to the expected destination
            mockTransportService.Verify(
                ts => ts.SendMessage(
                    It.Is<Message>(m => m.Content == messageContent && m.Headers.Destination == OtherEndpointUri),
                    OtherEndpointCredentials,
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }

        [Fact]
        public async Task MessagesCanExpire()
        {
            var mockTransportService = GivenMockTransportService();
            const string messageContent = "Hello, world!";
            using (var bus = await GivenConfiguredBus(mockTransportService.Object))
            {
                var options = new SendOptions
                {
                    TTL = TimeSpan.FromMinutes(15)
                };
                await bus.Send(messageContent, options);
            }

            // Only one call to ITransportService.SendMessage
            mockTransportService.Verify(
                ts => ts.SendMessage(
                    It.IsAny<Message>(),
                    It.IsAny<IEndpointCredentials>(),
                    It.IsAny<CancellationToken>()),
                Times.Once());

            // Specifically, only one call to ITransportService.SendMessage for this message
            // addressed to the expected destination
            mockTransportService.Verify(
                ts => ts.SendMessage(
                    It.Is<Message>(m => m.Content == messageContent &&
                                        m.Headers.Expires <= DateTime.UtcNow.AddMinutes(15)),
                    DefaultEndpointCredentials,
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }

        [Fact]
        public async Task MessageCanBeAddressedToSpecificEndpointUriWithOverridingCredentials()
        {
            var overridingCredentials = new BasicAuthCredentials("override", "overridepw");
            var mockTransportService = GivenMockTransportService();
            const string messageContent = "Hello, world!";
            using (var bus = await GivenConfiguredBus(mockTransportService.Object))
            {
                var options = new SendOptions
                {
                    Credentials = overridingCredentials
                };
                await bus.Send(messageContent, OtherEndpointUri, options);
            }

            // Only one call to ITransportService.SendMessage
            mockTransportService.Verify(
                ts => ts.SendMessage(
                    It.IsAny<Message>(),
                    It.IsAny<IEndpointCredentials>(),
                    It.IsAny<CancellationToken>()),
                Times.Once());

            // Specifically, only one call to ITransportService.SendMessage for this message
            // addressed to the expected destination with the specified override credentials
            mockTransportService.Verify(
                ts => ts.SendMessage(
                    It.Is<Message>(m => m.Content == messageContent &&
                                        m.Headers.Destination == OtherEndpointUri),
                    overridingCredentials,
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }

        [Fact]
        public async Task DefaultSendOptionsUsedWhenNoExplicitSendOptions()
        {
            var defaultSendOptionCredentials = new BasicAuthCredentials("default", "defaultpw");
            Configuration.DefaultSendOptions = new SendOptions
            {
                TTL = TimeSpan.FromMinutes(15),
                Synchronous = true,
                ContentType = "application/json",
                Credentials = defaultSendOptionCredentials
            };

            var mockTransportService = GivenMockTransportService();
            var content = new
            {
                message = "Hello, World!"
            };

            const string expectedMessageContent = "{\"message\":\"Hello, World!\"}";

            using (var bus = await GivenConfiguredBus(mockTransportService.Object))
            {
                await bus.Send(content);
            }

            // Only one call to ITransportService.SendMessage
            mockTransportService.Verify(
                ts => ts.SendMessage(
                    It.IsAny<Message>(),
                    It.IsAny<IEndpointCredentials>(),
                    It.IsAny<CancellationToken>()),
                Times.Once());

            // Specifically, only one call to ITransportService.SendMessage for this message
            // with header values based on the default credentials
            mockTransportService.Verify(
                ts => ts.SendMessage(
                    It.Is<Message>(m => m.Content == expectedMessageContent && 
                    m.Headers.Synchronous &&
                    m.Headers.Expires <= DateTime.UtcNow.AddMinutes(15)),
                    defaultSendOptionCredentials,
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }

        protected Mock<ITransportService> GivenMockTransportService()
        {
            var mockTransportService = new Mock<ITransportService>();
            mockTransportService.Setup(t => t.SendMessage(
                    It.IsAny<Message>(),
                    It.IsAny<IEndpointCredentials>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));

            return mockTransportService;
        }

        protected async Task<Bus> GivenConfiguredBus(ITransportService transportService)
        {
            var allMessages = new MessageNamePatternSpecification(".*");
            var noMatchSpecification = new Mock<IMessageSpecification>().Object;
            var messageQueueingService = new Mock<IMessageQueueingService>();
            
            Configuration.AddEndpoint(DefaultEndpointName, new Endpoint(DefaultEndpointUri, DefaultEndpointCredentials));
            Configuration.AddSendRule(new SendRule(allMessages, DefaultEndpointName));

            Configuration.AddEndpoint(OtherEndpointName, new Endpoint(OtherEndpointUri, OtherEndpointCredentials));
            Configuration.AddSendRule(new SendRule(noMatchSpecification, OtherEndpointName));

            var baseUri = new Uri("http://localhost/platibus");
            var bus = new Bus(Configuration, baseUri, transportService, messageQueueingService.Object);
            await bus.Init();
            return bus;
        }
    }
}
