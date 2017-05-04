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
        public async Task HandlingRulesAreAdded()
        {
            var configuration = await PlatibusConfigurationManager.LoadConfiguration();

            var handlingRules = configuration.HandlingRules.ToList();
            Assert.That(handlingRules.Count, Is.EqualTo(1));
            Assert.That(handlingRules[0].Specification, Is.InstanceOf<MessageNamePatternSpecification>());

            var namePatternSpec = (MessageNamePatternSpecification) handlingRules[0].Specification;
            Assert.That(namePatternSpec.NameRegex.ToString(), Is.EqualTo(".*(?i)example.*"));
            Assert.That(handlingRules[0].MessageHandler, Is.InstanceOf<MessageHandlerStub>());
        }
    }
}