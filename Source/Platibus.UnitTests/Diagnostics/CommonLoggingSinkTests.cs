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
using NLog;
using NLog.Config;
using NLog.Layouts;
using Platibus.Diagnostics;
using Xunit;
using Xunit.Abstractions;
using LogLevel = NLog.LogLevel;

namespace Platibus.UnitTests.Diagnostics
{
    /// <summary>
    /// These tests automate the execution of the test cases and perform basic string pattern
    /// matching to verify results.  However, manual review of test output is required to ensure
    /// that the results are as expected.
    /// </summary>
    [Trait("Category", "UnitTests")]
    public class CommonLoggingSinkTests
    {
        protected DiagnosticEventType Test = new DiagnosticEventType("Test");
        protected Message Message;
        protected Exception Exception;
        protected QueueName Queue;
        protected TopicName Topic;
        protected DiagnosticEvent Event;
        protected TestLog Log;

        public CommonLoggingSinkTests(ITestOutputHelper testOutputHelper)
        {
            var nlogConfig = new LoggingConfiguration();
            var layout = new SimpleLayout("${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff} ${level} ${message} ${onexception:inner=\\: ${exception:format=ToString}}");
            var target = new TestOutputHelperTarget(testOutputHelper)
            {
                Name = "test",
                Layout = layout
            };
            nlogConfig.AddTarget("test", target);
            nlogConfig.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
            var nlogLogger = new LogFactory(nlogConfig).GetLogger("test");
            Log = new TestLog(nlogLogger);
        }

        [Fact]
        public async Task EventsWithNoMessageAreLogged()
        {
            GivenNoMessage();
            GivenDiagnosticEvent();
            await WhenReceivingEvent();
            AssertMessageIsLogged();
        }

        [Fact]
        public async Task ExceptionIsIncludedInLogMessages()
        {
            GivenMessage();
            GivenException();
            GivenDiagnosticEvent();
            await WhenReceivingEvent();
            AssertMessageIsLogged();
        }

        [Fact]
        public async Task MessageIdIsIncludedInLogMessages()
        {
            GivenMessage();
            GivenDiagnosticEvent();
            await WhenReceivingEvent();
            AssertMessageIsLogged();
            AssertLogMessageContainsMessageId();
        }

        [Fact]
        public async Task QueueIsIncludedInLogMessages()
        {
            GivenMessage();
            Queue = "TestQueue";
            GivenDiagnosticEvent();
            await WhenReceivingEvent();
            AssertMessageIsLogged();
            AssertLogMessageContainsQueue();
        }

        [Fact]
        public async Task TopicIsIncludedInLogMessage()
        {
            GivenMessage();
            Topic = "TestTopic";
            GivenDiagnosticEvent();
            await WhenReceivingEvent();
            AssertMessageIsLogged();
            AssertLogMessageContainsTopic();
        }

        protected void GivenNoMessage()
        {
            Message = null;
        }

        protected void GivenMessage()
        {
            var messageHeaders = new MessageHeaders
            {
                MessageId = MessageId.Generate()
            };
            Message = new Message(messageHeaders, "Hello, world!");
        }

        protected void GivenException()
        {
            try
            {
                throw new Exception("Test exception");
            }
            catch (Exception e)
            {
                Exception = e;
            }
        }

        protected void GivenDiagnosticEvent()
        {
            Event = new DiagnosticEventBuilder(this, Test)
            {
                Message = Message,
                Detail = "This is a test event",
                Exception = Exception,
                Queue = Queue,
                Topic = Topic
            }.Build();
        }

        protected Task WhenReceivingEvent()
        {
            return new CommonLoggingSink(Log).Receive(Event);
        }
        
        protected void AssertMessageIsLogged()
        {
            Assert.True(Log.LoggedMessages.Any());
        }

        protected void AssertLogMessageContainsMessageId()
        {
            Assert.True(LoggedMessageContains("MessageId=" + Message.Headers.MessageId));
        }

        protected void AssertLogMessageContainsQueue()
        {
            Assert.True(LoggedMessageContains("Queue=" + Queue));
        }

        protected void AssertLogMessageContainsTopic()
        {
            Assert.True(LoggedMessageContains("Topic=" + Topic));
        }

        private bool LoggedMessageContains(string substring)
        {
            return Log.LoggedMessages
                .Where(x => x.Message != null)
                .Select(x => x.Message.ToString())
                .Any(msg => msg.Contains(substring));
        }
    }
}
