using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Platibus.Config;
using Xunit;

namespace Platibus.UnitTests.Config
{
    [Trait("Category", "UnitTests")]
    public class PlatibusConfigurationExtensionsTests
    {
        private class TestMessageNamingService : IMessageNamingService
        {
            public MessageName GetNameForType(Type messageType)
            {
                return messageType == typeof(A) ? "A" : "B";
            }

            public Type GetTypeForName(MessageName messageName)
            {
                return messageName == "A" ? typeof(A) : typeof(B);
            }
        }

        private class A
        { }

        private class B
        {
        }

        private class H : IMessageHandler<A>, IMessageHandler<B>
        {
            public async Task HandleMessage(A content, IMessageContext messageContext, CancellationToken cancellationToken)
            {
                await messageContext.SendReply("A", cancellationToken: cancellationToken);
            }

            public async Task HandleMessage(B content, IMessageContext messageContext, CancellationToken cancellationToken)
            {
                await messageContext.SendReply("B", cancellationToken: cancellationToken);
            }
        }

        [Fact]
        public async Task HandlingRulesAddedForMessageHandlerInterfaceImplementationsOfGenericTypeParameter()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };
            configuration.AddHandlingRules<H>();
            await AssertRulesAdded(configuration);
        }

        [Fact]
        public async Task QueueNameFactoryIsUsedToGenerateAssignQueueNamesForHandlers()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };

            configuration.AddHandlingRules<H>(queueNameFactory: (ht, mt) => "Q" + ht.Name + mt.Name);

            await AssertRulesAdded(configuration);

            var handlingRules = configuration.HandlingRules.ToList();
            Assert.Equal(2, handlingRules.Count);

            var expectedASpec = new MessageNamePatternSpecification(@"^A$");
            var aRules = handlingRules.Where(r => expectedASpec.Equals(r.Specification)).ToList();
            Assert.Equal(1, aRules.Count);
            Assert.Equal((QueueName)"QHA", aRules[0].QueueName);

            var expectedBSpec = new MessageNamePatternSpecification(@"^B$");
            var bRules = handlingRules.Where(r => expectedBSpec.Equals(r.Specification)).ToList();
            Assert.Equal(1, bRules.Count);
            Assert.Equal((QueueName)"QHB", bRules[0].QueueName);
        }

        [Fact]
        public async Task HandlingRulesAddedForMessageHandlerInterfaceImplementationsOfDelegateReturnType()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };
            configuration.AddHandlingRules(() => new H());

            await AssertRulesAdded(configuration);
        }

        [Fact]
        public async Task HandlingRulesAddedForMessageHandlerInterfaceImplementationsOfDelegateReturnType2()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };

            var h = new H();
            configuration.AddHandlingRules(() => h);

            await AssertRulesAdded(configuration);
        }

        [Fact]
        public async Task HandlingRulesAddedForMessageHandlerInterfaceImplementationsOfInstanceType()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };

            var h = new H();
            configuration.AddHandlingRules(h);

            await AssertRulesAdded(configuration);
        }

        [Fact]
        public async Task HandlingRulesAddedForMessageHandlerInterfaceImplementationsOfType()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };
            configuration.AddHandlingRulesForType(typeof(H));

            await AssertRulesAdded(configuration);
        }

        [Fact]
        public async Task HandlingRulesAddedForMessageHandlerInterfaceImplementationsOfTypeWithFactory()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };
            configuration.AddHandlingRulesForType(typeof(H), () => new H());

            await AssertRulesAdded(configuration);
        }

        [Fact]
        public async Task HandlingRulesAddedForMessageHandlerInterfaceImplementationsOfTypeWithFactory2()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };
            var h = new H();
            configuration.AddHandlingRulesForType(typeof(H), () => h);

            await AssertRulesAdded(configuration);
        }

        private static async Task AssertRulesAdded(IPlatibusConfiguration configuration)
        {
            var handlingRules = configuration.HandlingRules.ToList();
            Assert.Equal(2, handlingRules.Count);
            
            var expectedASpec = new MessageNamePatternSpecification(@"^A$");
            var aRules = handlingRules.Where(r => expectedASpec.Equals(r.Specification)).ToList();
            Assert.Equal(1, aRules.Count);

            var expectedBSpec = new MessageNamePatternSpecification(@"^B$");
            var bRules = handlingRules.Where(r => expectedBSpec.Equals(r.Specification)).ToList();
            Assert.Equal(1, bRules.Count);

            var mockContext = new Mock<IMessageContext>();
            var cancellationToken = default(CancellationToken);
            mockContext.Setup(x => x.SendReply("A", null, cancellationToken)).Returns(Task.FromResult(0)).Verifiable();
            mockContext.Setup(x => x.SendReply("B", null, cancellationToken)).Returns(Task.FromResult(0)).Verifiable();

            await aRules[0].MessageHandler.HandleMessage(new A(), mockContext.Object, cancellationToken);
            await bRules[0].MessageHandler.HandleMessage(new B(), mockContext.Object, cancellationToken);

            mockContext.Verify();
        }
    }
}
