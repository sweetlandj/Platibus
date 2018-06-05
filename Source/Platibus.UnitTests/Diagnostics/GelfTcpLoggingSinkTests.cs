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
    public class GelfTcpLoggingSinkTests : GelfLoggingSinkTests
    {
        protected const string Host = "localhost";
        protected const int Port = 12211;

        public GelfTcpLoggingSinkTests() 
            : base(new GelfTcpLoggingSink(Host, Port))
        {
        }

        [Fact]
        [Trait("Category", "Explicit")]
        public async Task MessagesCanBeHandledConcurrentlyAsync()
        {
            const int count = 10;
            var tasks = Enumerable
                .Range(0, count)
                .Select(i =>
                {
                    var messageNumber = i + 1;
                    var message = GenerateMessage();
                    var detail = "Concurrent message (" + messageNumber + " of " + count + ")";
                    var @event = GenerateDiagnosticEvent(this, TestExecuted, message, detail);

                    return GelfLoggingSink.ConsumeAsync(@event);
                });

            await Task.WhenAll(tasks);
        }

        [Fact]
        [Trait("Category", "Explicit")]
        public async Task MessagesCanBeHandledConcurrently()
        {
            const int count = 10;
            var tasks = Enumerable
                .Range(0, count)
                .Select(i => Task.Run(() =>
                {
                    var messageNumber = i + 1;
                    var message = GenerateMessage();
                    var detail = "Concurrent message (" + messageNumber + " of " + count + ")";
                    var @event = GenerateDiagnosticEvent(this, TestExecuted, message, detail);

                    GelfLoggingSink.Consume(@event);
                }));

            await Task.WhenAll(tasks);
        }
    }
}
