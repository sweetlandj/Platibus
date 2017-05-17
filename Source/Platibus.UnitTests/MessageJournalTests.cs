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
using Platibus.Journaling;
using Xunit;

namespace Platibus.UnitTests
{
    [Trait("Category", "UnitTests")]
    public abstract class MessageJournalTests
    {
        protected IMessageJournal MessageJournal;
        protected Message Message;

        protected MessageJournalTests(IMessageJournal messageJournal)
        {
            MessageJournal = messageJournal;
        }

        [Fact]
        public async Task SentMessagesShouldBeWrittenToJournal()
        {
            GivenSentMessage();
            await WhenAppendingSentMessage();
            await AssertSentMessageIsWrittenToJournal();
        }

        [Fact]
        public async Task ReceivedMessagesShouldBeWrittenToJournal()
        {
            GivenReceivedMessage();
            await WhenAppendingReceivedMessage();
            await AssertReceivedMessageIsWrittenToJournal();
        }

        [Fact]
        public async Task PublishedMessagesShouldBeWrittenToJournal()
        {
            GivenPublishedMessage();
            await WhenAppendingPublishedMessage();
            await AssertPublishedMessageIsWrittenToJournal();
        }
        
        protected Message GivenSentMessage()
        {
            var messageHeaders = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                Sent = DateTime.UtcNow,
                Origination = new Uri("urn:localhost/platibus0"),
                Destination = new Uri("urn:localhost/platibus1"),
                MessageName = "TestMessage",
                ContentType = "text/plain"
            };
            return Message = new Message(messageHeaders, "Hello, world!");
        }

        protected virtual Task WhenAppendingSentMessage()
        {
            return MessageJournal.Append(Message, JournaledMessageCategory.Sent);
        }

        protected virtual async Task AssertSentMessageIsWrittenToJournal()
        {
            var beginningOfJournal = await MessageJournal.GetBeginningOfJournal();
            var filter = new MessageJournalFilter
            {
                Categories = {JournaledMessageCategory.Sent}
            };
            var readResult = await MessageJournal.Read(beginningOfJournal, 100, filter);
            Assert.NotNull(readResult);
            var messages = readResult.JournaledMessages.Select(jm => jm.Message);
            Assert.Contains(Message, messages, new MessageEqualityComparer());
        }

        protected Message GivenReceivedMessage()
        {
            var messageHeaders = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                Sent = DateTime.UtcNow.AddSeconds(-1),
                Received = DateTime.UtcNow,
                Origination = new Uri("urn:localhost/platibus0"),
                Destination = new Uri("urn:localhost/platibus1"),
                MessageName = "TestMessage",
                ContentType = "text/plain"
            };
            return Message = new Message(messageHeaders, "Hello, world!");
        }

        protected virtual Task WhenAppendingReceivedMessage()
        {
            return MessageJournal.Append(Message, JournaledMessageCategory.Received);
        }

        protected virtual async Task AssertReceivedMessageIsWrittenToJournal()
        {
            var beginningOfJournal = await MessageJournal.GetBeginningOfJournal();
            var filter = new MessageJournalFilter
            {
                Categories = {JournaledMessageCategory.Received}
            };
            var readResult = await MessageJournal.Read(beginningOfJournal, 100, filter);
            Assert.NotNull(readResult);
            var messages = readResult.JournaledMessages.Select(jm => jm.Message);
            Assert.Contains(Message, messages, new MessageEqualityComparer());
        }

        protected Message GivenPublishedMessage()
        {
            var messageHeaders = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                Published = DateTime.UtcNow,
                Origination = new Uri("urn:localhost/platibus0"),
                MessageName = "TestMessage",
                ContentType = "text/plain",
                Topic = "TestTopic"
            };
            return Message = new Message(messageHeaders, "Hello, world!");
        }

        protected virtual Task WhenAppendingPublishedMessage()
        {
            return MessageJournal.Append(Message, JournaledMessageCategory.Published);
        }

        protected virtual async Task AssertPublishedMessageIsWrittenToJournal()
        {
            var beginningOfJournal = await MessageJournal.GetBeginningOfJournal();
            var filter = new MessageJournalFilter
            {
                Categories = {JournaledMessageCategory.Published},
                Topics = {Message.Headers.Topic}
            };
            var readResult = await MessageJournal.Read(beginningOfJournal, 100, filter);
            Assert.NotNull(readResult);
            var messages = readResult.JournaledMessages.Select(jm => jm.Message);
            Assert.Contains(Message, messages, new MessageEqualityComparer());
        }
    }
}