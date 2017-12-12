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


using System.Runtime.Serialization;
using Xunit;

namespace Platibus.UnitTests
{
    [Trait("Category", "UnitTests")]
    public class DataContractMessageNamingServiceTests
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
            Assert.Equal(typeof(ContractA), messageType);
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
            Assert.Equal(typeof(ContractB), messageType);
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
            Assert.Equal(typeof(ContractA), messageType);
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
            Assert.Equal(typeof(ContractB), messageType);
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
