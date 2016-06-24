using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Platibus.Config;

namespace Platibus.UnitTests
{
    class BusSendTests
    {
        
        [Test]
        public async Task SendRules_Critical_Importance_TransportService_Called()
        {
            var baseUri = new Uri("http://localhost:52180/platibus/");
            var endpointName = new EndpointName("local");
            var transportService = new Mock<ITransportService>();
            var messageQueueingService = new Mock<IMessageQueueingService>();
            var handler = new Mock<IMessageHandler>();
            var config = new PlatibusConfiguration
            {
                DefaultContentType = "text/plain"
            };

            var allMessages = new MessageNamePatternSpecification(".*");
            var handlerQueueName = new QueueName("handler");

            config.AddEndpoint(endpointName, new Endpoint(baseUri));
            config.AddSendRule(new SendRule(allMessages, endpointName));
            config.AddHandlingRule(new HandlingRule(allMessages, handler.Object, handlerQueueName));

            transportService.Setup(
                t => t.SendMessage(It.IsAny<Message>(), It.IsAny<IEndpointCredentials>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            var bus = new Bus(config, baseUri, transportService.Object, messageQueueingService.Object);
            await bus.Init();

            const string messageContent = "Hello, world!";
            await bus.Send(messageContent, new SendOptions { Importance = MessageImportance.Critical });

            transportService.Verify(
                t => t.SendMessage(It.IsAny<Message>(), It.IsAny<IEndpointCredentials>(), It.IsAny<CancellationToken>()),
                Times.Once());
            
            messageQueueingService.Verify(
                mqs =>
                    mqs.EnqueueMessage(handlerQueueName, It.IsAny<Message>(),
                        It.IsAny<IPrincipal>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Test]
        public async Task SendRules_Normal_Importance_TransportService_Called()
        {
            var baseUri = new Uri("http://localhost:52180/platibus/");
            var endpointName = new EndpointName("local");
            var transportService = new Mock<ITransportService>();
            var messageQueueingService = new Mock<IMessageQueueingService>();
            var handler = new Mock<IMessageHandler>();
            var config = new PlatibusConfiguration
            {
                DefaultContentType = "text/plain"
            };

            var allMessages = new MessageNamePatternSpecification(".*");
            var handlerQueueName = new QueueName("handler");

            config.AddEndpoint(endpointName, new Endpoint(baseUri));
            config.AddSendRule(new SendRule(allMessages, endpointName));
            config.AddHandlingRule(new HandlingRule(allMessages, handler.Object, handlerQueueName));

            transportService.Setup(
                t => t.SendMessage(It.IsAny<Message>(), It.IsAny<IEndpointCredentials>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            var bus = new Bus(config, baseUri, transportService.Object, messageQueueingService.Object);
            await bus.Init();

            const string messageContent = "Hello, world!";
            await bus.Send(messageContent, new SendOptions { Importance = MessageImportance.Normal });

            transportService.Verify(
                ts => ts.SendMessage(It.Is<Message>(m => m.Content == messageContent), It.IsAny<IEndpointCredentials>(), It.IsAny<CancellationToken>()),
                Times.Once());

            messageQueueingService.Verify(
                mqs =>
                    mqs.EnqueueMessage(It.IsAny<QueueName>(), It.IsAny<Message>(),
                        It.IsAny<IPrincipal>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Test]
        public async Task Send_EndpointName_Critical_Importance_TransportService_Called()
        {
            var baseUri = new Uri("http://localhost:52180/platibus/");
            var endpointName = new EndpointName("local");
            var transportService = new Mock<ITransportService>();
            var messageQueueingService = new Mock<IMessageQueueingService>();
            var handler = new Mock<IMessageHandler>();
            var config = new PlatibusConfiguration
            {
                DefaultContentType = "text/plain"
            };

            var allMessages = new MessageNamePatternSpecification(".*");
            var handlerQueueName = new QueueName("handler");

            config.AddEndpoint(endpointName, new Endpoint(baseUri));
            config.AddSendRule(new SendRule(allMessages, endpointName));
            config.AddHandlingRule(new HandlingRule(allMessages, handler.Object, handlerQueueName));

            transportService.Setup(
                 t => t.SendMessage(It.IsAny<Message>(), It.IsAny<IEndpointCredentials>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.FromResult(true));

            var bus = new Bus(config, baseUri, transportService.Object, messageQueueingService.Object);
            await bus.Init();

            const string messageContent = "Hello, world!";
            await bus.Send(messageContent, endpointName, new SendOptions { Importance = MessageImportance.Critical });

            transportService.Verify(
                ts => ts.SendMessage(It.Is<Message>(m => m.Content == messageContent), It.IsAny<IEndpointCredentials>(), It.IsAny<CancellationToken>()),
                Times.Once());

            messageQueueingService.Verify(
                mqs =>
                    mqs.EnqueueMessage(It.IsAny<QueueName>(), It.IsAny<Message>(),
                        It.IsAny<IPrincipal>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Test]
        public async Task Send_EndpointName_Normal_Importance_TransportService_Called()
        {
            var baseUri = new Uri("http://localhost:52180/platibus/");
            var endpointName = new EndpointName("local");
            var transportService = new Mock<ITransportService>();
            var messageQueueingService = new Mock<IMessageQueueingService>();
            var handler = new Mock<IMessageHandler>();
            var config = new PlatibusConfiguration
            {
                DefaultContentType = "text/plain"
            };

            var allMessages = new MessageNamePatternSpecification(".*");
            var handlerQueueName = new QueueName("handler");

            config.AddEndpoint(endpointName, new Endpoint(baseUri));
            config.AddSendRule(new SendRule(allMessages, endpointName));
            config.AddHandlingRule(new HandlingRule(allMessages, handler.Object, handlerQueueName));

            transportService.Setup(
                t => t.SendMessage(It.IsAny<Message>(), It.IsAny<IEndpointCredentials>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            var bus = new Bus(config, baseUri, transportService.Object, messageQueueingService.Object);
            await bus.Init();

            const string messageContent = "Hello, world!";
            await bus.Send(messageContent, endpointName, new SendOptions { Importance = MessageImportance.Normal });

            transportService.Verify(
                ts => ts.SendMessage(It.Is<Message>(m => m.Content == messageContent), It.IsAny<IEndpointCredentials>(), It.IsAny<CancellationToken>()),
                Times.Once());

            messageQueueingService.Verify(
                mqs =>
                    mqs.EnqueueMessage(It.IsAny<QueueName>(), It.IsAny<Message>(),
                        It.IsAny<IPrincipal>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Test]
        public async Task Send_EndpointAddress_Critical_Importance_TransportService_Called()
        {
            var baseUri = new Uri("http://localhost:52180/platibus/");
            var endpointName = new EndpointName("local");
            var transportService = new Mock<ITransportService>();
            var messageQueueingService = new Mock<IMessageQueueingService>();
            var handler = new Mock<IMessageHandler>();
            var config = new PlatibusConfiguration
            {
                DefaultContentType = "text/plain"
            };

            var allMessages = new MessageNamePatternSpecification(".*");
            var handlerQueueName = new QueueName("handler");

            config.AddEndpoint(endpointName, new Endpoint(baseUri));
            config.AddSendRule(new SendRule(allMessages, endpointName));
            config.AddHandlingRule(new HandlingRule(allMessages, handler.Object, handlerQueueName));

            transportService.Setup(
                t => t.SendMessage(It.IsAny<Message>(), It.IsAny<IEndpointCredentials>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            var bus = new Bus(config, baseUri, transportService.Object, messageQueueingService.Object);
            await bus.Init();

            const string messageContent = "Hello, world!";
            await bus.Send(messageContent, baseUri, options: new SendOptions { Importance = MessageImportance.Critical });

            transportService.Verify(
                ts => ts.SendMessage(It.Is<Message>(m => m.Content == messageContent), It.IsAny<IEndpointCredentials>(), It.IsAny<CancellationToken>()),
                Times.Once());

            messageQueueingService.Verify(
                mqs =>
                    mqs.EnqueueMessage(It.IsAny<QueueName>(), It.IsAny<Message>(),
                        It.IsAny<IPrincipal>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Test]
        public async Task Send_EndpointAddress_Normal_Importance_TransportService_Called()
        {
            var baseUri = new Uri("http://localhost:52180/platibus/");
            var endpointName = new EndpointName("local");
            var transportService = new Mock<ITransportService>();
            var messageQueueingService = new Mock<IMessageQueueingService>();
            var handler = new Mock<IMessageHandler>();
            var config = new PlatibusConfiguration
            {
                DefaultContentType = "text/plain"
            };

            var allMessages = new MessageNamePatternSpecification(".*");
            var handlerQueueName = new QueueName("handler");

            config.AddEndpoint(endpointName, new Endpoint(baseUri));
            config.AddSendRule(new SendRule(allMessages, endpointName));
            config.AddHandlingRule(new HandlingRule(allMessages, handler.Object, handlerQueueName));

            transportService.Setup(
                t => t.SendMessage(It.IsAny<Message>(), It.IsAny<IEndpointCredentials>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            var bus = new Bus(config, baseUri, transportService.Object, messageQueueingService.Object);
            await bus.Init();

            const string messageContent = "Hello, world!";
            await bus.Send(messageContent, baseUri, options: new SendOptions { Importance = MessageImportance.Normal });

            transportService.Verify(
                ts => ts.SendMessage(It.Is<Message>(m => m.Content == messageContent), It.IsAny<IEndpointCredentials>(), It.IsAny<CancellationToken>()),
                Times.Once());

            messageQueueingService.Verify(
                mqs =>
                    mqs.EnqueueMessage(It.IsAny<QueueName>(), It.IsAny<Message>(),
                        It.IsAny<IPrincipal>(), It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}
