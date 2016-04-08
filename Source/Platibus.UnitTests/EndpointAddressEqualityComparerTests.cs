
using System;
using NUnit.Framework;

namespace Platibus.UnitTests
{
    public class EndpointAddressEqualityComparerTests
    {
        [Test]
        public void IdenticalEndpointAddressesShouldBeEqual()
        {
            var address1 = new Uri("http://test.example.com:8080/platibus/");
            var address2 = new Uri("http://test.example.com:8080/platibus/");
            Assert.That(address1, Is.EqualTo(address2).Using(new EndpointAddressEqualityComparer()));
        }

        [Test]
        public void EndpointAddressesShouldBeEqualWithoutTerminatingPathSeparator()
        {
            var address1 = new Uri("http://test.example.com:8080/platibus");
            var address2 = new Uri("http://test.example.com:8080/platibus/");
            Assert.That(address1, Is.EqualTo(address2).Using(new EndpointAddressEqualityComparer()));
        }

        [Test]
        public void EndpointAddressesWithDifferentQueryStringsShouldBeEqual()
        {
            var address1 = new Uri("http://test.example.com:8080/platibus/");
            var address2 = new Uri("http://test.example.com:8080/platibus/?test=true");
            Assert.That(address1, Is.EqualTo(address2).Using(new EndpointAddressEqualityComparer()));
        }

        [Test]
        public void EndpointAddressesShouldBeEqualWithDifferentQueryStringsAndWithoutTerminatingPathSeparator()
        {
            var address1 = new Uri("http://test.example.com:8080/platibus");
            var address2 = new Uri("http://test.example.com:8080/platibus/?test=true");
            Assert.That(address1, Is.EqualTo(address2).Using(new EndpointAddressEqualityComparer()));
        }

        [Test]
        public void EndpointAddressesWithDifferentHostsShouldNotBeEqual()
        {
            var address1 = new Uri("http://test1.example.com:8080/platibus/");
            var address2 = new Uri("http://test2.example.com:8080/platibus/");
            Assert.That(address1, Is.Not.EqualTo(address2).Using(new EndpointAddressEqualityComparer()));
        }

        [Test]
        public void EndpointAddressesWithDifferentSchemesShouldNotBeEqual()
        {
            var address1 = new Uri("https://test.example.com:8080/platibus/");
            var address2 = new Uri("http://test.example.com:8080/platibus/");
            Assert.That(address1, Is.Not.EqualTo(address2).Using(new EndpointAddressEqualityComparer()));
        }

        [Test]
        public void EndpointAddressesWithDifferentPathsShouldNotBeEqual()
        {
            var address1 = new Uri("http://test.example.com:8080/platibus1/");
            var address2 = new Uri("http://test.example.com:8080/platibus2/");
            Assert.That(address1, Is.Not.EqualTo(address2).Using(new EndpointAddressEqualityComparer()));
        }

        [Test]
        public void EndpointAddressesWithDifferentAuthoritiesShouldNotBeEqual()
        {
            var address1 = new Uri("http://user:pass@test.example.com:8080/platibus1/");
            var address2 = new Uri("http://test.example.com:8080/platibus2/");
            Assert.That(address1, Is.Not.EqualTo(address2).Using(new EndpointAddressEqualityComparer()));
        }
    }
}
