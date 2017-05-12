
using System;
using Xunit;

namespace Platibus.UnitTests
{
    [Trait("Category", "UnitTests")]
    public class EndpointAddressEqualityComparerTests
    {
        [Fact]
        public void IdenticalEndpointAddressesShouldBeEqual()
        {
            var address1 = new Uri("http://test.example.com:8080/platibus/");
            var address2 = new Uri("http://test.example.com:8080/platibus/");
            Assert.Equal(address1, address2, new EndpointAddressEqualityComparer());
        }

        [Fact]
        public void EndpointAddressesShouldBeEqualWithoutTerminatingPathSeparator()
        {
            var address1 = new Uri("http://test.example.com:8080/platibus");
            var address2 = new Uri("http://test.example.com:8080/platibus/");
            Assert.Equal(address1, address2, new EndpointAddressEqualityComparer());
        }

        [Fact]
        public void EndpointAddressesWithDifferentQueryStringsShouldBeEqual()
        {
            var address1 = new Uri("http://test.example.com:8080/platibus/");
            var address2 = new Uri("http://test.example.com:8080/platibus/?test=true");
            Assert.Equal(address1, address2, new EndpointAddressEqualityComparer());
        }

        [Fact]
        public void EndpointAddressesShouldBeEqualWithDifferentQueryStringsAndWithoutTerminatingPathSeparator()
        {
            var address1 = new Uri("http://test.example.com:8080/platibus");
            var address2 = new Uri("http://test.example.com:8080/platibus/?test=true");
            Assert.Equal(address1, address2, new EndpointAddressEqualityComparer());
        }

        [Fact]
        public void EndpointAddressesWithDifferentHostsShouldNotBeEqual()
        {
            var address1 = new Uri("http://test1.example.com:8080/platibus/");
            var address2 = new Uri("http://test2.example.com:8080/platibus/");
            Assert.NotEqual(address1, address2, new EndpointAddressEqualityComparer());
        }

        [Fact]
        public void EndpointAddressesWithDifferentSchemesShouldNotBeEqual()
        {
            var address1 = new Uri("https://test.example.com:8080/platibus/");
            var address2 = new Uri("http://test.example.com:8080/platibus/");
            Assert.NotEqual(address1, address2, new EndpointAddressEqualityComparer());
        }

        [Fact]
        public void EndpointAddressesWithDifferentPathsShouldNotBeEqual()
        {
            var address1 = new Uri("http://test.example.com:8080/platibus1/");
            var address2 = new Uri("http://test.example.com:8080/platibus2/");
            Assert.NotEqual(address1, address2, new EndpointAddressEqualityComparer());
        }

        [Fact]
        public void EndpointAddressesWithDifferentAuthoritiesShouldNotBeEqual()
        {
            var address1 = new Uri("http://user:pass@test.example.com:8080/platibus1/");
            var address2 = new Uri("http://test.example.com:8080/platibus2/");
            Assert.NotEqual(address1, address2, new EndpointAddressEqualityComparer());
        }
    }
}
