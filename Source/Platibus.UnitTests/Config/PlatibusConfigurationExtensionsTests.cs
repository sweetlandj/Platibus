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
        {
        }

        private class B
        {
        }
        
        private class H : IMessageHandler<A>, IMessageHandler<B>
        {
            async Task IMessageHandler<A>.HandleMessage(A content, IMessageContext messageContext, CancellationToken cancellationToken)
            {
                await messageContext.SendReply("A", cancellationToken: cancellationToken);
            }

            async Task IMessageHandler<B>.HandleMessage(B content, IMessageContext messageContext, CancellationToken cancellationToken)
            {
                await messageContext.SendReply("B", cancellationToken: cancellationToken);
            }
        }

        [Fact]
        public async Task HandlingRuleAddedForImplicitNamePatterns()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };
            var aHandler = new H() as IMessageHandler<A>;
            configuration.AddHandlingRule("^A$", aHandler);

            var bHandler = new H() as IMessageHandler<B>;
            configuration.AddHandlingRule("^B$", bHandler);
            await AssertRulesAdded(configuration);
        }

        [Fact]
        public async Task HandlingRuleAddedForImplicitNamePatternsWithExplicitQueueNames()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };
            var aHandler = new H() as IMessageHandler<A>;
            configuration.AddHandlingRule("^A$", aHandler, "QHA");

            var bHandler = new H() as IMessageHandler<B>;
            configuration.AddHandlingRule("^B$", bHandler, "QHB");

            await AssertRulesAdded(configuration);
            AssertExplicitQueueNames(configuration);
        }

        [Fact]
        public async Task HandlingRuleAddedForImplicitNamePatternsWithPerInstanceHandlerFactories()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };
            configuration.AddHandlingRule("^A$", () => new H() as IMessageHandler<A>);
            configuration.AddHandlingRule("^B$", () => new H() as IMessageHandler<B>);

            await AssertRulesAdded(configuration);
        }

        [Fact]
        public async Task HandlingRuleAddedForImplicitNamePatternsWithSingletonHandlerFactories()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };
            var aHandler = new H() as IMessageHandler<A>;
            configuration.AddHandlingRule("^A$", () => aHandler);

            var bHandler = new H() as IMessageHandler<B>;
            configuration.AddHandlingRule("^B$", () => bHandler);

            await AssertRulesAdded(configuration);
        }

        [Fact]
        public async Task HandlingRuleAddedForImplicitNamePatternsWithHandlerFactoriesAndExplicitQueueNames()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };
            var aHandler = new H() as IMessageHandler<A>;
            configuration.AddHandlingRule("^A$", () => aHandler, "QHA");

            var bHandler = new H() as IMessageHandler<B>;
            configuration.AddHandlingRule("^B$", () => bHandler, "QHB");

            await AssertRulesAdded(configuration);
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
            AssertExplicitQueueNames(configuration);
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

        private static void AssertExplicitQueueNames(IPlatibusConfiguration configuration)
        {
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
    }
}
