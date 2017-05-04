using NUnit.Framework;

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

        [Test]
        public void MessageTypeCanBeResolvedForUnregisteredTypes()
        {
            var messageNamingService = new DefaultMessageNamingService();
            var messageType = messageNamingService.GetTypeForName("Platibus.UnitTests.DefaultMessageNamingServiceTests+ContractA");
            Assert.That(messageType, Is.Not.Null);
            Assert.That(messageType, Is.EqualTo(typeof(ContractA)));
        }

        [Test]
        public void MessageNameCanBeResolvedForUnregisteredTypes()
        {
            var messageNamingService = new DefaultMessageNamingService();
            var messageName = messageNamingService.GetNameForType(typeof(ContractA));
            Assert.That(messageName, Is.Not.Null);
            Assert.That((string)messageName, Is.EqualTo("Platibus.UnitTests.DefaultMessageNamingServiceTests+ContractA"));
        }
    }
}