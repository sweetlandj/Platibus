using Platibus.Config.Extensibility;
using Xunit;

namespace Platibus.UnitTests.Config.Extensibility
{
    public class ProviderHelperTests
    {
        protected IFakeProvider Provider;

        [Fact]
        public void MultipleProvidersFoundExceptionThrownWhenMultipleProvidersWithSameNameAndInterface()
        {
            Assert.Throws<MultipleProvidersFoundException>(() => WhenGetttingProvider("Multiple"));
        }

        [Fact]
        public void ProviderNotFoundExceptionThrownWhenNoProviderWithNameAndInterface()
        {
            Assert.Throws<ProviderNotFoundException>(() => WhenGetttingProvider("DoesNotExist"));
        }

        [Fact]
        public void SingleProviderFoundByName()
        {
            WhenGetttingProvider("Single");
            Assert.IsType<ProviderFakes.SingleProvider>(Provider);
        }

        protected void WhenGetttingProvider(string providerName)
        {
            Provider = ProviderHelper.GetProvider<IFakeProvider>(providerName);
        }
    }
}
