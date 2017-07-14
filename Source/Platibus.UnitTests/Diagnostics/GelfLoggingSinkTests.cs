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
using System.Threading.Tasks;
using Platibus.Diagnostics;
using Xunit;

namespace Platibus.UnitTests.Diagnostics
{
    [Trait("Category", "UnitTests")]
    [Trait("Dependency", "Graylog2")]
    public abstract class GelfLoggingSinkTests
    {
        protected static readonly DiagnosticEventType TestExecuted = new DiagnosticEventType("TestExecuted", DiagnosticEventLevel.Info);
        protected GelfLoggingSink GelfLoggingSink;
        protected Message Message;
        protected string Detail = "This is a test";
        protected Exception Exception;
        protected QueueName Queue;
        protected TopicName Topic;
        protected DiagnosticEvent Event;

        protected GelfLoggingSinkTests(GelfLoggingSink gelfLoggingSink)
        {
            GelfLoggingSink = gelfLoggingSink;
        }

        [Fact]
        public async Task GelfMessagesAreSent()
        {
            GivenMessage();
            GivenDiagnosticEvent();
            await WhenReceivingEvent();
        }

        protected void GivenNoMessage()
        {
            Message = null;
        }

        protected void GivenMessage()
        {
            Message = GenerateMessage();
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
            Event = GenerateDiagnosticEvent(this, TestExecuted, Message, Detail, Queue, Topic, Exception);
        }

        protected Task WhenReceivingEvent()
        {
            return GelfLoggingSink.ConsumeAsync(Event);
        }

        protected static DiagnosticEvent GenerateDiagnosticEvent(object source, DiagnosticEventType type, 
            Message message = null, string detail = null, QueueName queue = null, TopicName topic = null, 
            Exception ex = null)
        {
            return new DiagnosticEventBuilder(source, type)
            {
                Message = message,
                Detail = detail,
                Exception = ex,
                Queue = queue,
                Topic = topic
            }.Build();
        }

        protected static Message GenerateMessage()
        {
            var messageHeaders = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                Sent = DateTime.UtcNow.AddSeconds(-1),
                Received = DateTime.UtcNow,
                Origination = new Uri("http://localhost/platibus"),
                Destination = new Uri("http://localhost:81/platibus"),
                MessageName = "Test:TestMessage",
                RelatedTo = MessageId.Generate()
            };
            return new Message(messageHeaders, "Hello, world!");
        }
    }
}
