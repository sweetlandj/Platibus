// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.


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
