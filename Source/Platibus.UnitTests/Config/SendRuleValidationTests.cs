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
using System.Linq;
using Moq;
using Xunit;
using Platibus.Config;

namespace Platibus.UnitTests.Config
{
    [Trait("Category", "UnitTests")]
    public class SendRuleValidationTests
    {
        protected PlatibusConfiguration Configuration;

        [Fact]
        public void SendRuleWithKnownEndpointIsValid()
        {
            GivenValidConfiguration();
            GivenSendRuleWithUnknownEndpoint();
            Assert.Throws<InvalidSendRuleException>(() => WhenValidating());
        }

        [Fact]
        public void SendRuleWithNoEndpointsIsInvalid()
        {
            GivenValidConfiguration();
            GivenSendRuleWithNoEndpoints();
            Assert.Throws<InvalidSendRuleException>(() => WhenValidating());
        }

        [Fact]
        public void SendRuleWithUnknownEndpointIsInvalid()
        {
            GivenValidConfiguration();
            GivenSendRuleWithKnownEndpoint();
            AssertDoesNotThrow(WhenValidating);
        }

        private void GivenValidConfiguration()
        {
            Configuration = new PlatibusConfiguration();
        }

        private void GivenSendRuleWithNoEndpoints()
        {
            var mockSendRule = new Mock<ISendRule>();
            mockSendRule.Setup(x => x.Specification).Returns(new MessageNamePatternSpecification(".*"));
            mockSendRule.Setup(x => x.Endpoints).Returns(Enumerable.Empty<EndpointName>());
            Configuration.AddSendRule(mockSendRule.Object);
        }

        private void GivenSendRuleWithUnknownEndpoint()
        {
            var endpointName = Guid.NewGuid().ToString();
            var sendRule = new SendRule(new MessageNamePatternSpecification(".*"), endpointName);
            Configuration.AddSendRule(sendRule);
        }

        private void GivenSendRuleWithKnownEndpoint()
        {
            var endpointName = Guid.NewGuid().ToString();
            var mockEndpoint = new Mock<IEndpoint>();
            Configuration.AddEndpoint(endpointName, mockEndpoint.Object);
            var sendRule = new SendRule(new MessageNamePatternSpecification(".*"), endpointName);
            Configuration.AddSendRule(sendRule);
        }

        private void WhenValidating()
        {
            Configuration.Validate();
        }

        /// <summary>
        /// Helper method to improve readability
        /// </summary>
        /// <param name="action">The action that does not throw</param>
        private static void AssertDoesNotThrow(Action action)
        {
            action();
        }
    }
}
