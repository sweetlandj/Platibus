using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Platibus.Config;

namespace Platibus.UnitTests
{
    public class BusInitTests
    {
        protected Uri BaseUri = new Uri("http://localhost:52180/platibus/");
        protected PlatibusConfiguration Configuration;
        protected IMessageQueueingService MessageQueueingService;
        protected ITransportService TransportService;

        [Test]
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

        private Mock<ITransportService> GivenMockTransportService()
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
            return mockTransportService;
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
