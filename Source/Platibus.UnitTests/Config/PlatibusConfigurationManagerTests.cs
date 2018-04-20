using System;
using Platibus.Config;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Platibus.UnitTests.Config
{
    [Trait("Category", "UnitTests")]
    public class PlatibusConfigurationManagerTests
    {
        protected PlatibusConfiguration Configuration;
        
        [Fact]
        public async Task HandlingRulesAreAdded()
        {
            GivenEmptyConfiguration();
            await WhenProcessingConfigurationHooks();
            AssertHandlingRulesAreAdded();
        }

        protected void GivenEmptyConfiguration()
        {
            Configuration = new PlatibusConfiguration();
        }

        protected Task WhenProcessingConfigurationHooks()
        {
            var configurationManager = new PlatibusConfigurationManager();
#pragma warning disable 612
            return configurationManager.FindAndProcessConfigurationHooks(Configuration);
#pragma warning restore 612
        }

        protected void AssertHandlingRulesAreAdded()
        {
            var handlingRules = Configuration.HandlingRules.ToList();
            Assert.Single(handlingRules);
            Assert.IsType<MessageNamePatternSpecification>(handlingRules[0].Specification);

            var namePatternSpec = (MessageNamePatternSpecification)handlingRules[0].Specification;
            Assert.Equal(".*(?i)example.*", namePatternSpec.NameRegex.ToString());
            Assert.IsType<MessageHandlerStub>(handlingRules[0].MessageHandler);
        }
        
        [Obsolete]
        public class TestConfigurationHook : IConfigurationHook
        {
            public void Configure(PlatibusConfiguration configuration)
            {
                configuration.AddHandlingRule(".*(?i)example.*", new MessageHandlerStub());
            }
        }
    }
}