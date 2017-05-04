using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Platibus.Config;
using Platibus.Security;

namespace Platibus.UnitTests
{
    internal class BusSendTests
    {
        protected readonly EndpointName DefaultEndpointName = "local";
        protected readonly Uri DefaultEndpointUri = new Uri("http://localhost/default/");
        protected readonly IEndpointCredentials DefaultEndpointCredentials = new BasicAuthCredentials("default", "defaultpw");

        protected readonly EndpointName OtherEndpointName = "other";
        protected readonly Uri OtherEndpointUri = new Uri("http://localhost/other/");
        protected readonly IEndpointCredentials OtherEndpointCredentials = new BasicAuthCredentials("other", "otherpw");

        [Test]
        public async Task SendRulesDetermineMessageDestination()
        {
            var mockTransportService =  GivenMockTransportService();
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
        
        [Test]
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

        [Test]
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

        [Test]
        public async Task MessageCanBeAddressedToSpecificEndpointUriWithOverridingCredentials()
        {
            var overridingCredentials = new BasicAuthCredentials("override", "overridepw");
            var mockTransportService = GivenMockTransportService();
            const string messageContent = "Hello, world!";
            using (var bus = await GivenConfiguredBus(mockTransportService.Object))
            {
                await bus.Send(messageContent, OtherEndpointUri, overridingCredentials);
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
                    It.Is<Message>(m => m.Content == messageContent && m.Headers.Destination == OtherEndpointUri),
                    overridingCredentials,
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

            var config = new PlatibusConfiguration
            {
                DefaultContentType = "text/plain"
            };

            config.AddEndpoint(DefaultEndpointName, new Endpoint(DefaultEndpointUri, DefaultEndpointCredentials));
            config.AddSendRule(new SendRule(allMessages, DefaultEndpointName));

            config.AddEndpoint(OtherEndpointName, new Endpoint(OtherEndpointUri, OtherEndpointCredentials));
            config.AddSendRule(new SendRule(noMatchSpecification, OtherEndpointName));
            
            var baseUri = new Uri("http://localhost/platibus");
            var bus = new Bus(config, baseUri, transportService, messageQueueingService.Object);
            await bus.Init();
            return bus;
        }
    }
}
