using System.Threading;
using System.Threading.Tasks;
using Moq;
using Platibus.Config.Extensibility;
using Platibus.Diagnostics;
using Xunit;

namespace Platibus.UnitTests.Config.Extensibility
{
    public class ReflectionBaseProviderServiceTests
    {
        protected Mock<IDiagnosticService> MockDiagnosticService;
        protected ReflectionBasedProviderService ProviderService;
        protected IFakeProvider Provider;

        public ReflectionBaseProviderServiceTests()
        {
            MockDiagnosticService = new Mock<IDiagnosticService>();
            MockDiagnosticService.Setup(s => s.EmitAsync(It.IsAny<DiagnosticEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));

            ProviderService = new ReflectionBasedProviderService(MockDiagnosticService.Object);
        }

        [Fact]
        public void MultipleProvidersFoundExceptionThrownWhenMultipleProvidersWithSameNameAndInterface()
        {
            Assert.Throws<MultipleProvidersFoundException>(() => WhenGettingProvider("Multiple"));
        }

        [Fact]
        public void ProviderNotFoundExceptionThrownWhenNoProviderWithNameAndInterface()
        {
            Assert.Throws<ProviderNotFoundException>(() => WhenGettingProvider("DoesNotExist"));
        }

        [Fact]
        public void SingleProviderFoundByName()
        {
            WhenGettingProvider("Single");
            Assert.IsType<ProviderFakes.SingleProvider>(Provider);
        }

        protected void WhenGettingProvider(string providerName)
        {
            Provider = ProviderService.GetProvider<IFakeProvider>(providerName);
        }
    }
}
