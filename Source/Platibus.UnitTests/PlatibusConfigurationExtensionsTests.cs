
using Moq;
using NUnit.Framework;
using Platibus.Config;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.UnitTests
{
    internal class PlatibusConfigurationExtensionsTests
    {
        private class TestMessageNamingService : IMessageNamingService
        {
            public MessageName GetNameForType(Type messageType)
            {
                return messageType == typeof (A) ? "A" : "B";
            }

            public Type GetTypeForName(MessageName messageName)
            {
                return messageName == "A" ? typeof (A) : typeof (B);
            }
        }

        private class A
        { }

        private class B
        {
        }

        private class H : IMessageHandler<A>, IMessageHandler<B>
        {
            public Task HandleMessage(A content, IMessageContext messageContext, CancellationToken cancellationToken)
            {
                return Task.FromResult("A");
            }

            public Task HandleMessage(B content, IMessageContext messageContext, CancellationToken cancellationToken)
            {
                return Task.FromResult("B");
            }
        }

        [Test]
        public void When_AddHandlingRules_Generic_No_Factory_Then_Expected_Rules_Added()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };
            configuration.AddHandlingRules<H>();

            AssertRulesAdded(configuration);
        }

        [Test]
        public void When_AddHandlingRules_Generic_No_Factory_QueueNameFactory_Then_Expected_Rules_Added()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };

            configuration.AddHandlingRules<H>(queueNameFactory: (ht, mt) => "Q" + ht.Name + mt.Name);

            AssertRulesAdded(configuration);

            var handlingRules = configuration.HandlingRules.ToList();
            Assert.That(handlingRules.Count, Is.EqualTo(2));

            var expectedASpec = new MessageNamePatternSpecification(@"^A$");
            var aRules = handlingRules.Where(r => expectedASpec.Equals(r.Specification)).ToList();
            Assert.That(aRules.Count, Is.EqualTo(1));
            Assert.That(aRules[0].QueueName, Is.EqualTo((QueueName)"QHA"));

            var expectedBSpec = new MessageNamePatternSpecification(@"^B$");
            var bRules = handlingRules.Where(r => expectedBSpec.Equals(r.Specification)).ToList();
            Assert.That(bRules.Count, Is.EqualTo(1));
            Assert.That(bRules[0].QueueName, Is.EqualTo((QueueName)"QHB"));
        }

        [Test]
        public void When_AddHandlingRules_Generic_Factory_Then_Expected_Rules_Added()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };
            configuration.AddHandlingRules(() => new H());

            AssertRulesAdded(configuration);
        }

        [Test]
        public void When_AddHandlingRules_Generic_Singleton_Factory_Then_Expected_Rules_Added()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };

            var h = new H();
            configuration.AddHandlingRules(() => h);

            AssertRulesAdded(configuration);
        }

        [Test]
        public void When_AddHandlingRules_Singleton_Instance_Then_Expected_Rules_Added()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };

            var h = new H();
            configuration.AddHandlingRules(h);

            AssertRulesAdded(configuration);
        }

        [Test]
        public void When_AddHandlingRulesForType_No_Factory_Then_Expected_Rules_Added()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };
            configuration.AddHandlingRulesForType(typeof(H));

            AssertRulesAdded(configuration);
        }

        [Test]
        public void When_AddHandlingRulesForType_Factory_Then_Expected_Rules_Added()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };
            configuration.AddHandlingRulesForType(typeof(H), () => new H());

            AssertRulesAdded(configuration);
        }

        [Test]
        public void When_AddHandlingRulesForType_Singleton_Factory_Then_Expected_Rules_Added()
        {
            var configuration = new PlatibusConfiguration
            {
                MessageNamingService = new TestMessageNamingService()
            };
            var h = new H();
            configuration.AddHandlingRulesForType(typeof(H), () => h);

            AssertRulesAdded(configuration);
        }

        private static void AssertRulesAdded(IPlatibusConfiguration configuration)
        {
            var handlingRules = configuration.HandlingRules.ToList();
            Assert.That(handlingRules.Count, Is.EqualTo(2));

            var expectedASpec = new MessageNamePatternSpecification(@"^A$");
            var aRules = handlingRules.Where(r => expectedASpec.Equals(r.Specification)).ToList();
            Assert.That(aRules.Count, Is.EqualTo(1));
            var aResult = aRules[0].MessageHandler.HandleMessage(new A(), new Mock<IMessageContext>().Object,
                default(CancellationToken));
            Assert.That(aResult, Is.InstanceOf<Task<string>>());
            Assert.That(((Task<string>)aResult).Result, Is.EqualTo("A"));

            var expectedBSpec = new MessageNamePatternSpecification(@"^B$");
            var bRules = handlingRules.Where(r => expectedBSpec.Equals(r.Specification)).ToList();
            Assert.That(bRules.Count, Is.EqualTo(1));
            var bResult = bRules[0].MessageHandler.HandleMessage(new B(), new Mock<IMessageContext>().Object,
                default(CancellationToken));
            Assert.That(bResult, Is.InstanceOf<Task<string>>());
            Assert.That(((Task<string>)bResult).Result, Is.EqualTo("B"));
        }
    }
}
