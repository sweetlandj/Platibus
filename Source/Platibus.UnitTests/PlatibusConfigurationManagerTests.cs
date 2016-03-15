using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Platibus.Config;

namespace Platibus.UnitTests
{
    public class PlatibusConfigurationManagerTests
    {
        public class TestConfigurationHook : IConfigurationHook
        {
            public void Configure(PlatibusConfiguration configuration)
            {
                configuration.AddHandlingRule(".*(?i)example.*", new MessageHandlerStub());
            }
        }

        [Test]
        public async Task Given_Configuration_Hook_When_Loading_Configuration_Then_Message_Route_Added()
        {
            var configuration = await PlatibusConfigurationManager.LoadConfiguration();

            var messageRoutes = configuration.HandlingRules.ToList();
            Assert.That(messageRoutes.Count, Is.EqualTo(1));
            Assert.That(messageRoutes[0].Specification, Is.InstanceOf<MessageNamePatternSpecification>());

            var namePatternSpec = (MessageNamePatternSpecification) messageRoutes[0].Specification;
            Assert.That(namePatternSpec.NameRegex.ToString(), Is.EqualTo(".*(?i)example.*"));

            Assert.That(messageRoutes[0].MessageHandler, Is.InstanceOf<MessageHandlerStub>());
        }
    }
}