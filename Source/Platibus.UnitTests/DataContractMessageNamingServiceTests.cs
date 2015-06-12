
using System.Runtime.Serialization;
using NUnit.Framework;

namespace Platibus.UnitTests
{
    class DataContractMessageNamingServiceTests
    {
        [DataContract]
        public class ContractA
        {
            
        }

        [DataContract(Namespace = "http://platibus/unittests", Name = "Contract-B")]
        public class ContractB
        {
            
        }

        [Test]
        public void Can_Resolve_MessageType_For_Known_DataContract_With_Defaults()
        {
            var messageNamingService = new DataContractMessageNamingService();
            messageNamingService.Add(typeof(ContractA));
            var messageType = messageNamingService.GetTypeForName("http://schemas.datacontract.org/2004/07/Platibus.UnitTests:DataContractMessageNamingServiceTests.ContractA");
            Assert.That(messageType, Is.Not.Null);
            Assert.That(messageType, Is.EqualTo(typeof(ContractA)));
        }

        [Test]
        public void Can_Resolve_MessageName_For_Known_DataContract_With_Defaults()
        {
            var messageNamingService = new DataContractMessageNamingService();
            messageNamingService.Add(typeof(ContractA));
            var messageName = messageNamingService.GetNameForType(typeof(ContractA));
            Assert.That(messageName, Is.Not.Null);
            Assert.That((string)messageName, Is.EqualTo("http://schemas.datacontract.org/2004/07/Platibus.UnitTests:DataContractMessageNamingServiceTests.ContractA"));
        }

        [Test]
        public void Can_Resolve_MessageType_For_Known_DataContract_With_Explicit_Name()
        {
            var messageNamingService = new DataContractMessageNamingService();
            messageNamingService.Add(typeof(ContractB));
            var messageType = messageNamingService.GetTypeForName("http://platibus/unittests:Contract-B");
            Assert.That(messageType, Is.Not.Null);
            Assert.That(messageType, Is.EqualTo(typeof(ContractB)));
        }

        [Test]
        public void Can_Resolve_MessageName_For_Known_DataContract_With_Explicit_Name()
        {
            var messageNamingService = new DataContractMessageNamingService();
            messageNamingService.Add(typeof(ContractB));
            var messageName = messageNamingService.GetNameForType(typeof(ContractB));
            Assert.That(messageName, Is.Not.Null);
            Assert.That((string)messageName, Is.EqualTo("http://platibus/unittests:Contract-B"));
        }

        [Test]
        public void Can_Resolve_MessageType_For_Unknown_DataContract_With_Defaults()
        {
            var messageNamingService = new DataContractMessageNamingService();
            var messageType = messageNamingService.GetTypeForName("http://schemas.datacontract.org/2004/07/Platibus.UnitTests:DataContractMessageNamingServiceTests.ContractA");
            Assert.That(messageType, Is.Not.Null);
            Assert.That(messageType, Is.EqualTo(typeof(ContractA)));
        }

        [Test]
        public void Can_Resolve_MessageName_For_Unknown_DataContract_With_Defaults()
        {
            var messageNamingService = new DataContractMessageNamingService();
            var messageName = messageNamingService.GetNameForType(typeof(ContractA));
            Assert.That(messageName, Is.Not.Null);
            Assert.That((string)messageName, Is.EqualTo("http://schemas.datacontract.org/2004/07/Platibus.UnitTests:DataContractMessageNamingServiceTests.ContractA"));
        }

        [Test]
        public void Can_Resolve_MessageType_For_Unknown_DataContract_With_Explicit_Name()
        {
            var messageNamingService = new DataContractMessageNamingService();
            var messageType = messageNamingService.GetTypeForName("http://platibus/unittests:Contract-B");
            Assert.That(messageType, Is.Not.Null);
            Assert.That(messageType, Is.EqualTo(typeof(ContractB)));
        }

        [Test]
        public void Can_Resolve_MessageName_For_Unknown_DataContract_With_Explicit_Name()
        {
            var messageNamingService = new DataContractMessageNamingService();
            var messageName = messageNamingService.GetNameForType(typeof(ContractB));
            Assert.That(messageName, Is.Not.Null);
            Assert.That((string)messageName, Is.EqualTo("http://platibus/unittests:Contract-B"));
        }
    }
}
