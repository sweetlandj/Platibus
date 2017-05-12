
using System.Runtime.Serialization;
using Xunit;

namespace Platibus.UnitTests
{
    internal class DataContractMessageNamingServiceTests
    {
        [DataContract]
        public class ContractA
        {

        }

        [DataContract(Namespace = "http://platibus/unittests", Name = "Contract-B")]
        public class ContractB
        {

        }

        [Fact]
        public void MessageTypeCanBeResolvedForRegisteredTypes()
        {
            var messageNamingService = new DataContractMessageNamingService();
            messageNamingService.Add(typeof(ContractA));
            var messageType = messageNamingService.GetTypeForName("http://schemas.datacontract.org/2004/07/Platibus.UnitTests:DataContractMessageNamingServiceTests.ContractA");
            Assert.NotNull(messageType);
            Assert.IsType<ContractA>(messageType);
        }

        [Fact]
        public void MessageNameCanBeResolvedForRegisteredTypes()
        {
            var messageNamingService = new DataContractMessageNamingService();
            messageNamingService.Add(typeof(ContractA));
            var messageName = messageNamingService.GetNameForType(typeof(ContractA));
            Assert.NotNull(messageName);
            Assert.Equal("http://schemas.datacontract.org/2004/07/Platibus.UnitTests:DataContractMessageNamingServiceTests.ContractA", messageName);
        }

        [Fact]
        public void MessageTypeCanBeResolvedForRegisteredTypesWithExplicitNames()
        {
            var messageNamingService = new DataContractMessageNamingService();
            messageNamingService.Add(typeof(ContractB));
            var messageType = messageNamingService.GetTypeForName("http://platibus/unittests:Contract-B");
            Assert.NotNull(messageType);
            Assert.IsType<ContractB>(messageType);
        }

        [Fact]
        public void MessageNameCanBeResolvedForRegisteredTypesWithExplicitNames()
        {
            var messageNamingService = new DataContractMessageNamingService();
            messageNamingService.Add(typeof(ContractB));
            var messageName = messageNamingService.GetNameForType(typeof(ContractB));
            Assert.NotNull(messageName);
            Assert.Equal("http://platibus/unittests:Contract-B", messageName);
        }

        [Fact]
        public void MessageTypeCanBeResolvedForUnregisteredTypes()
        {
            var messageNamingService = new DataContractMessageNamingService();
            var messageType = messageNamingService.GetTypeForName("http://schemas.datacontract.org/2004/07/Platibus.UnitTests:DataContractMessageNamingServiceTests.ContractA");
            Assert.NotNull(messageType);
            Assert.IsType<ContractA>(messageType);
        }

        [Fact]
        public void MessageNameCanBeResolvedForUnregisteredTypes()
        {
            var messageNamingService = new DataContractMessageNamingService();
            var messageName = messageNamingService.GetNameForType(typeof(ContractA));
            Assert.NotNull(messageName);
            Assert.Equal("http://schemas.datacontract.org/2004/07/Platibus.UnitTests:DataContractMessageNamingServiceTests.ContractA", messageName);
        }

        [Fact]
        public void MessageTypeCanBeResolvedForUnregisteredTypesWithExplicitNames()
        {
            var messageNamingService = new DataContractMessageNamingService();
            var messageType = messageNamingService.GetTypeForName("http://platibus/unittests:Contract-B");
            Assert.NotNull(messageType);
            Assert.IsType<ContractB>(messageType);
        }

        [Fact]
        public void MessageNameCanBeResolvedForUnregisteredTypesWithExplicitNames()
        {
            var messageNamingService = new DataContractMessageNamingService();
            var messageName = messageNamingService.GetNameForType(typeof(ContractB));
            Assert.NotNull(messageName);
            Assert.Equal("http://platibus/unittests:Contract-B", messageName);
        }
    }
}
