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
using System.Threading.Tasks;
using Platibus.Diagnostics;
using Xunit;

namespace Platibus.UnitTests.Diagnostics
{
    /// <summary>
    /// These tests automate the execution of the test cases but require manual verification in
    /// the target log collector service.
    /// </summary>
    [Trait("Category", "UnitTests")]
    [Trait("Dependency", "Graylog2")]
    public class GelfUdpLoggingSinkTests : GelfLoggingSinkTests
    {
        protected const string Host = "localhost";
        protected const int Port = 12201;

        public GelfUdpLoggingSinkTests() 
            : base(new GelfUdpLoggingSink(Host, Port))
        {
        }

        [Fact]
        public async Task LargeMessagesShouldBeChunked()
        {
            GivenMessageWithAllHeaders();
            Queue = "TestQueue";
            GivenException();
            GivenLengthyDetailMessage();
            GivenDiagnosticEvent();
            await WhenReceivingEvent();
        }

        [Fact]
        public async Task MessagesCanBeCompressed()
        {
            GivenMessageWithAllHeaders();
            Queue = "TestQueue";
            GivenException();
            GivenLengthyDetailMessage();
            GivenDiagnosticEvent();
            GivenCompressionEnabled();
            await WhenReceivingEvent();
        }

        protected void GivenLengthyDetailMessage()
        {
            var paragraphs = Enumerable.Range(1, 20).Select(i =>
                i + ". Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed" +
                " do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut " +
                "enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi " +
                "ut aliquip ex ea commodo consequat. Duis aute irure dolor in " +
                "reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla " +
                "pariatur. Excepteur sint occaecat cupidatat non proident, sunt in " +
                "culpa qui officia deserunt mollit anim id est laborum.");

            var paragraphSeparator = Environment.NewLine + Environment.NewLine;
            Detail = string.Join(paragraphSeparator, paragraphs);
        }

        protected void GivenCompressionEnabled()
        {
            GelfLoggingSink = new GelfUdpLoggingSink(Host, Port, true);
        }

        protected void GivenMessageWithAllHeaders()
        {
            var messageHeaders = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                MessageName = "LoremIpsum",
                RelatedTo = MessageId.Generate(),
                Origination = new Uri("http://localhost:80/platibus"),
                Destination = new Uri("http://localhost:81/platibus")
            };
            Message = new Message(messageHeaders, "Hello, world!");
        }
    }
}
