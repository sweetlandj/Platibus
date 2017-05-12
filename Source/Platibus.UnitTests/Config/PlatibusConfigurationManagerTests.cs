using System.Linq;
using System.Threading.Tasks;
using Platibus.Config;
using Xunit;

namespace Platibus.UnitTests.Config
{
    [Trait("Category", "UnitTests")]
    public class PlatibusConfigurationManagerTests
    {
        public class TestConfigurationHook : IConfigurationHook
        {
            public void Configure(PlatibusConfiguration configuration)
            {
                configuration.AddHandlingRule(".*(?i)example.*", new MessageHandlerStub());
            }
        }

       [Fact]
        public async Task HandlingRulesAreAdded()
        {
            var configuration = await PlatibusConfigurationManager.LoadConfiguration();

            var handlingRules = configuration.HandlingRules.ToList();
            Assert.Equal(1, handlingRules.Count);
            Assert.IsType<MessageNamePatternSpecification>(handlingRules[0].Specification);

            var namePatternSpec = (MessageNamePatternSpecification) handlingRules[0].Specification;
            Assert.Equal(".*(?i)example.*", namePatternSpec.NameRegex.ToString());
            Assert.IsType<MessageHandlerStub>(handlingRules[0].MessageHandler);
        }
    }
}