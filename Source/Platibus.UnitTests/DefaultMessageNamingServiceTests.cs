using Xunit;

namespace Platibus.UnitTests
{
    internal class DefaultMessageNamingServiceTests
    {
        public class ContractA
        {

        }

        public class ContractB
        {

        }

        [Fact]
        public void MessageTypeCanBeResolvedForUnregisteredTypes()
        {
            var messageNamingService = new DefaultMessageNamingService();
            var messageType = messageNamingService.GetTypeForName("Platibus.UnitTests.DefaultMessageNamingServiceTests+ContractA");
            Assert.NotNull(messageType);
            Assert.IsType<ContractA>(messageType);
        }

        [Fact]
        public void MessageNameCanBeResolvedForUnregisteredTypes()
        {
            var messageNamingService = new DefaultMessageNamingService();
            var messageName = messageNamingService.GetNameForType(typeof(ContractA));
            Assert.NotNull(messageName);
            Assert.Equal("Platibus.UnitTests.DefaultMessageNamingServiceTests+ContractA", messageName);
        }
    }
}