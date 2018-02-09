using Moq;
using Platibus.Config;
using Platibus.Diagnostics;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Platibus.UnitTests.Config
{
    [Trait("Category", "UnitTests")]
    public class PlatibusConfigurationExtensionsTests
    {
        protected Mock<IDiagnosticService> MockDiagnosticService;
        protected PlatibusConfiguration Configuration;
        protected Exception Exception;

        public PlatibusConfigurationExtensionsTests()
        {
            MockDiagnosticService = new Mock<IDiagnosticService>();
            MockDiagnosticService
                .Setup(x => x.EmitAsync(It.IsAny<DiagnosticEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));

            Configuration = new PlatibusConfiguration(MockDiagnosticService.Object)
            {
                MessageNamingService = new TestMessageNamingService()
            };
        }

        [Fact]
        public async Task HandlingRuleAddedForImplicitNamePatterns()
        {
            var aHandler = new H() as IMessageHandler<A>;
            Configuration.AddHandlingRule("^A$", aHandler);

            var bHandler = new H() as IMessageHandler<B>;
            Configuration.AddHandlingRule("^B$", bHandler);
            await AssertRulesAdded();
        }

        [Fact]
        public async Task HandlingRuleAddedForImplicitNamePatternsWithExplicitQueueNames()
        {
            var aHandler = new H() as IMessageHandler<A>;
            Configuration.AddHandlingRule("^A$", aHandler, "QHA");

            var bHandler = new H() as IMessageHandler<B>;
            Configuration.AddHandlingRule("^B$", bHandler, "QHB");

            await AssertRulesAdded();
            AssertExplicitQueueNames();
        }

        [Fact]
        public async Task HandlingRuleAddedForImplicitNamePatternsWithPerInstanceHandlerFactories()
        {
            Configuration.AddHandlingRule("^A$", () => new H() as IMessageHandler<A>);
            Configuration.AddHandlingRule("^B$", () => new H() as IMessageHandler<B>);

            await AssertRulesAdded();
        }

        [Fact]
        public async Task HandlingRuleAddedForImplicitNamePatternsWithSingletonHandlerFactories()
        {
            var aHandler = new H() as IMessageHandler<A>;
            Configuration.AddHandlingRule("^A$", () => aHandler);

            var bHandler = new H() as IMessageHandler<B>;
            Configuration.AddHandlingRule("^B$", () => bHandler);

            await AssertRulesAdded();
        }

        [Fact]
        public async Task HandlingRuleAddedForImplicitNamePatternsWithHandlerFactoriesAndExplicitQueueNames()
        {
            var aHandler = new H() as IMessageHandler<A>;
            Configuration.AddHandlingRule("^A$", () => aHandler, "QHA");

            var bHandler = new H() as IMessageHandler<B>;
            Configuration.AddHandlingRule("^B$", () => bHandler, "QHB");

            await AssertRulesAdded();
        }

        [Fact]
        public async Task HandlingRulesAddedForMessageHandlerInterfaceImplementationsOfGenericTypeParameter()
        {
            Configuration.AddHandlingRules<H>();
            await AssertRulesAdded();
        }

        [Fact]
        public async Task QueueNameFactoryIsUsedToGenerateAssignQueueNamesForHandlers()
        {
            Configuration.AddHandlingRules<H>(queueNameFactory: (ht, mt) => "Q" + ht.Name + mt.Name);

            await AssertRulesAdded();
            AssertExplicitQueueNames();
        }

        [Fact]
        public async Task HandlingRulesAddedForMessageHandlerInterfaceImplementationsOfDelegateReturnType()
        {
            Configuration.AddHandlingRules(() => new H());
            await AssertRulesAdded();
        }

        [Fact]
        public async Task HandlingRulesAddedForMessageHandlerInterfaceImplementationsOfDelegateReturnType2()
        {
            var h = new H();
            Configuration.AddHandlingRules(() => h);
            await AssertRulesAdded();
        }

        [Fact]
        public async Task HandlingRulesAddedForMessageHandlerInterfaceImplementationsOfInstanceType()
        {
            var h = new H();
            Configuration.AddHandlingRules(h);
            await AssertRulesAdded();
        }

        [Fact]
        public async Task HandlingRulesAddedForMessageHandlerInterfaceImplementationsOfType()
        {
            Configuration.AddHandlingRulesForType(typeof(H));
            await AssertRulesAdded();
        }

        [Fact]
        public async Task HandlingRulesAddedForMessageHandlerInterfaceImplementationsOfTypeWithFactory()
        {
            Configuration.AddHandlingRulesForType(typeof(H), () => new H());
            await AssertRulesAdded();
        }

        [Fact]
        public async Task HandlingRulesAddedForMessageHandlerInterfaceImplementationsOfTypeWithFactory2()
        {
            var h = new H();
            Configuration.AddHandlingRulesForType(typeof(H), () => h);
            await AssertRulesAdded();
        }

        [Fact]
        public async Task UncaughtHandlerErrorsEmitDiagnosticEvents()
        {
            Configuration.AddHandlingRulesForType(typeof(H), () => throw new Exception());

            await WhenHandlerResolutionErrorsOccur();
            AssertDiagnosticEventEmitted();
        }

        private async Task WhenHandlerResolutionErrorsOccur()
        {
            try
            {
                await TryHandleMessages();
            }
            catch (Exception e)
            {
                Exception = e;
            }
        }

        private void AssertDiagnosticEventEmitted()
        {
            Assert.NotNull(Exception);
            MockDiagnosticService.Verify(x => x.Emit(It.Is<DiagnosticEvent>(e => IsHandlerActivationError(e))), Times.AtLeastOnce);
        }

        private bool IsHandlerActivationError(DiagnosticEvent e)
        {
            Assert.NotNull(e);
            Assert.Equal(DiagnosticEventType.HandlerActivationError, e.Type);
            Assert.Equal(Exception, e.Exception);
            Assert.StartsWith("Error activating instance of message handler ", e.Detail);
            return true;
        }

        private async Task AssertRulesAdded()
        {
            var handlingRules = Configuration.HandlingRules.ToList();
            Assert.Equal(2, handlingRules.Count);
            
            var expectedASpec = new MessageNamePatternSpecification(@"^A$");
            var aRules = handlingRules.Where(r => expectedASpec.Equals(r.Specification)).ToList();
            Assert.Single(aRules);

            var expectedBSpec = new MessageNamePatternSpecification(@"^B$");
            var bRules = handlingRules.Where(r => expectedBSpec.Equals(r.Specification)).ToList();
            Assert.Single(bRules);

            var mockContext = new Mock<IMessageContext>();
            var cancellationToken = default(CancellationToken);
            mockContext.Setup(x => x.SendReply("A", null, cancellationToken)).Returns(Task.FromResult(0)).Verifiable();
            mockContext.Setup(x => x.SendReply("B", null, cancellationToken)).Returns(Task.FromResult(0)).Verifiable();

            await aRules[0].MessageHandler.HandleMessage(new A(), mockContext.Object, cancellationToken);
            await bRules[0].MessageHandler.HandleMessage(new B(), mockContext.Object, cancellationToken);

            mockContext.Verify();
        }

        private async Task TryHandleMessages()
        {
            var handlingRules = Configuration.HandlingRules.ToList();
            Assert.Equal(2, handlingRules.Count);
            
            var expectedASpec = new MessageNamePatternSpecification(@"^A$");
            var aRules = handlingRules.Where(r => expectedASpec.Equals(r.Specification)).ToList();
            Assert.Single(aRules);

            var expectedBSpec = new MessageNamePatternSpecification(@"^B$");
            var bRules = handlingRules.Where(r => expectedBSpec.Equals(r.Specification)).ToList();
            Assert.Single(bRules);

            var mockContext = new Mock<IMessageContext>();
            var cancellationToken = default(CancellationToken);

            await aRules[0].MessageHandler.HandleMessage(new A(), mockContext.Object, cancellationToken);
            await bRules[0].MessageHandler.HandleMessage(new B(), mockContext.Object, cancellationToken);
        }

        private void AssertExplicitQueueNames()
        {
            var handlingRules = Configuration.HandlingRules.ToList();
            Assert.Equal(2, handlingRules.Count);

            var expectedASpec = new MessageNamePatternSpecification(@"^A$");
            var aRules = handlingRules.Where(r => expectedASpec.Equals(r.Specification)).ToList();
            Assert.Single(aRules);
            Assert.Equal((QueueName)"QHA", aRules[0].QueueName);

            var expectedBSpec = new MessageNamePatternSpecification(@"^B$");
            var bRules = handlingRules.Where(r => expectedBSpec.Equals(r.Specification)).ToList();
            Assert.Single(bRules);
            Assert.Equal((QueueName)"QHB", bRules[0].QueueName);
        }

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

    }
}
