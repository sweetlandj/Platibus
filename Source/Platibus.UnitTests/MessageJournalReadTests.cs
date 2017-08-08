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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Platibus.Journaling;
using Xunit;

namespace Platibus.UnitTests
{
    [Trait("Category", "UnitTests")]
    public abstract class MessageJournalReadTests
    {
        protected IMessageJournal MessageJournal;
        protected MessageJournalPosition Start;
        protected int Count;
        protected MessageJournalFilter Filter;
        protected MessageId FirstCommandMessageId = MessageId.Generate();
        
        protected IList<MessageJournalReadResult> Pages = new List<MessageJournalReadResult>();

        protected MessageJournalReadTests(IMessageJournal messageJournal)
        {
            MessageJournal = messageJournal;
        }

        [Fact]
        protected async Task ReadResultsCanBePaged()
        {
            await GivenSampleSet();
            await GivenAtBeginningOfJournal();
            GivenPageSize(10);
            await WhenReadingToEndOfJournal();

            // Expect all 32 messages to be read in 4 pages
            Assert.Equal(4, Pages.Count);
            Assert.Equal(Count, Pages[0].Entries.Count());
            Assert.Equal(Count, Pages[1].Entries.Count());
            Assert.Equal(Count, Pages[2].Entries.Count());
            Assert.Equal(2, Pages[3].Entries.Count());
        }

        [Fact]
        protected async Task ReadsAreRepeatable()
        {
            await GivenSampleSet();
            await GivenAtBeginningOfJournal();
            GivenPageSize(10);
            await WhenReadingToEndOfJournal();

            var firstRun = new List<MessageJournalReadResult>(Pages);
            Pages.Clear();

            await GivenAtBeginningOfJournal();
            await WhenReadingToEndOfJournal();

            var secondRun = new List<MessageJournalReadResult>(Pages);

            AssertSameResults(firstRun, secondRun);
        }

        [Theory]
        [InlineData("Sent", 8)]
        [InlineData("Received", 16)]
        [InlineData("Published", 8)]
        protected async Task ReadResultsCanBeFilteredByCategory(string category, int expectedCount)
        {
            await GivenSampleSet();
            await GivenAtBeginningOfJournal();
            GivenPageSize(10);
            GivenFilter(f => f.Categories.Add(category));
            await WhenReadingToEndOfJournal();

            var actual = Pages.SelectMany(p => p.Entries).ToList();
            Assert.Equal(expectedCount, actual.Count);
           
            AssertCategory(actual, category);
        }

        [Theory]
        [InlineData("FooEvents", 4)]
        [InlineData("BarEvents", 4)]
        [InlineData("BazEvents", 8)]
        protected async Task ReadResultsCanBeFilteredByTopic(string topic, int expectedCount)
        {
            await GivenSampleSet();
            await GivenAtBeginningOfJournal();
            GivenPageSize(10);
            GivenFilter(f => f.Topics.Add(topic));
            await WhenReadingToEndOfJournal();

            var actual = Pages.SelectMany(p => p.Entries).ToList();
            Assert.Equal(expectedCount, actual.Count);

            AssertTopic(actual, topic);
        }

        [Theory]
        [InlineData("Received", "FooEvents", 2)]
        [InlineData("Published", "BazEvents", 4)]
        protected async Task ReadResultsCanBeFilteredByCategoryAndTopic(string category, string topic, int expectedCount)
        {
            await GivenSampleSet();
            await GivenAtBeginningOfJournal();
            GivenPageSize(10);
            GivenFilter(f =>
            {
                f.Categories.Add(category);
                f.Topics.Add(topic);
            });
            await WhenReadingToEndOfJournal();

            var actual = Pages.SelectMany(p => p.Entries).ToList();
            Assert.Equal(expectedCount, actual.Count);

            AssertTopic(actual, topic);
        }

        [Theory]
        [InlineData("http://localhost/platibus0", 8)]
        [InlineData("http://localhost/platibus1", 24)]
        [InlineData("http://LocalHost/platibus0", 8)]
        [InlineData("http://LOCALHOST/platibus1", 24)]
        [InlineData("http://LocalHost/platibus0/", 8)]
        [InlineData("http://LOCALHOST/platibus1/", 24)]
        protected async Task ReadResultsCanBeFilteredByOrigination(string origination, int expectedCount)
        {
            await GivenSampleSet();
            await GivenAtBeginningOfJournal();
            GivenPageSize(10);
            GivenFilter(f => f.Origination = new Uri(origination));
            await WhenReadingToEndOfJournal();

            var actual = Pages.SelectMany(p => p.Entries).ToList();
            Assert.Equal(expectedCount, actual.Count);

            var expectedOrigination = new Uri(origination).WithTrailingSlash();
            AssertOrigination(actual, expectedOrigination);
        }
        
        [Theory]
        [InlineData("http://localhost/platibus0", 16)]
        [InlineData("http://localhost/platibus1", 8)]
        [InlineData("http://LocalHost/platibus0", 16)]
        [InlineData("http://LOCALHOST/platibus1", 8)]
        [InlineData("http://LocalHost/platibus0/", 16)]
        [InlineData("http://LOCALHOST/platibus1/", 8)]
        protected async Task ReadResultsCanBeFilteredByDestination(string destination, int expectedCount)
        {
            await GivenSampleSet();
            await GivenAtBeginningOfJournal();
            GivenPageSize(10);
            GivenFilter(f => f.Destination = new Uri(destination));
            await WhenReadingToEndOfJournal();

            var actual = Pages.SelectMany(p => p.Entries).ToList();
            Assert.Equal(expectedCount, actual.Count);

            var expectedDestination = new Uri(destination).WithTrailingSlash();
            AssertDestination(actual, expectedDestination);
        }

        [Fact]
        protected async Task ReadResultsCanBeFilteredByRelatedTo()
        {
            await GivenSampleSet();
            await GivenAtBeginningOfJournal();
            GivenPageSize(10);
            GivenFilter(f => f.RelatedTo = FirstCommandMessageId);
            await WhenReadingToEndOfJournal();

            var actual = Pages.SelectMany(p => p.Entries).ToList();
            Assert.Equal(2, actual.Count); // Journaled as "Sent" and as "Received"

            AssertRelatedTo(actual, FirstCommandMessageId);
        }

        [Theory]
        [InlineData("Foo", 8)]
        [InlineData("BarRequest", 2)]
        [InlineData("Response", 8)]
        [InlineData("Complete", 6)]
        [InlineData("Fail", 2)]
        protected async Task ReadResultsCanBeFilteredByMessageName(string messageName, int expectedCount)
        {
            await GivenSampleSet();
            await GivenAtBeginningOfJournal();
            GivenPageSize(10);
            GivenFilter(f => f.MessageName = messageName);
            await WhenReadingToEndOfJournal();

            var actual = Pages.SelectMany(p => p.Entries).ToList();
            Assert.Equal(expectedCount, actual.Count);

            AssertMessageName(actual, messageName);
        }

        protected async Task GivenSampleSet()
        {
            // Sample Set
            // ----------
            // 32 Messages
            //  8 Sent
            // 16 Received (16 direct, 8 publications)
            //  8 Published
            //  4 Messages with topic "FooEvents"
            //  4 Messages with topic "BarEvents"
            //  8 Messages with topic "BazEvents"
            //  8 Messages with origination "http://localhost/platibus0" (commands)
            // 24 Messages with origination "http://localhost/platibus1" (publications & replies)

            var fooRequest1 = await GivenJournaledSentMessage("FooRequest", "FooRequest:1", FirstCommandMessageId);
            await GivenJournaledReceivedMessage("FooRequest", "FooRequest:1");
            await GivenJournaledPublication("FooStarted", "FooStarted:1", "FooEvents");
            await GivenJournaledReceivedPublication("FooStarted", "FooStarted:1", "FooEvents");
            await GivenJournaledPublication("FooCompleted", "FooCompleted:1", "FooEvents");
            await GivenJournaledReceivedPublication("FooCompleted", "FooCompleted:1", "FooEvents");
            await GivenJournaledSentReply("FooResponse", "FooResponse:1", fooRequest1.Headers.MessageId);
            await GivenJournaledReceivedReply("FooResponse", "FooResponse:1", fooRequest1.Headers.MessageId);
            var barRequest1 = await GivenJournaledSentMessage("BarRequest", "BarRequest:1");
            await GivenJournaledReceivedMessage("BarRequest", "BarRequest:1");
            await GivenJournaledPublication("BarStarted", "BarStarted:1", "BarEvents");
            await GivenJournaledPublication("BarCompleted", "BarCompleted:1", "BarEvents");
            await GivenJournaledSentReply("BarResponse", "BarResponse:1", barRequest1.Headers.MessageId);
            await GivenJournaledReceivedPublication("BarStarted", "BarStarted:1", "BarEvents");
            var bazRequest1 = await GivenJournaledSentMessage("BazRequest", "BazRequest:1");
            await GivenJournaledReceivedReply("BarResponse", "BarResponse:1", barRequest1.Headers.MessageId);
            await GivenJournaledReceivedMessage("BazRequest", "BazRequest:1");
            await GivenJournaledReceivedPublication("BarCompleted", "BarCompleted:1", "BarEvents");
            await GivenJournaledPublication("BazStarted", "BazStarted:1", "BazEvents");
            await GivenJournaledReceivedPublication("BazStarted", "BazStarted:1", "BazEvents");
            await GivenJournaledPublication("BazFailed", "BazFailed:1", "BazEvents");
            await GivenJournaledSentReply("BazResponse", "BazResponse:1", bazRequest1.Headers.MessageId);
            await GivenJournaledReceivedReply("BazResponse", "BazResponse:1", bazRequest1.Headers.MessageId);
            await GivenJournaledReceivedPublication("BazFailed", "BazFailed:1", "BazEvents");
            var bazRequest2 = await GivenJournaledSentMessage("BazRequest", "BazRequest:2");
            await GivenJournaledReceivedMessage("BazRequest", "BazRequest:2");
            await GivenJournaledPublication("BazStarted", "BazStarted:2", "BazEvents");
            await GivenJournaledReceivedPublication("BazStarted", "BazStarted:2", "BazEvents");
            await GivenJournaledPublication("BazCompleted", "BazCompleted:2", "BazEvents");
            await GivenJournaledReceivedPublication("BazCompleted", "BazCompleted:2", "BazEvents");
            await GivenJournaledSentReply("BazResponse", "BazResponse:2", bazRequest2.Headers.MessageId);
            await GivenJournaledReceivedReply("BazResponse", "BazResponse:2", bazRequest2.Headers.MessageId);
        }

        protected async Task GivenAtBeginningOfJournal()
        {
            Start = await MessageJournal.GetBeginningOfJournal();
        }

        protected void GivenPageSize(int count)
        {
            Count = count;
        }

        protected async Task<Message> GivenJournaledSentMessage(string messageName, string content, MessageId messageId = default(MessageId))
        {
            var myMessageId = messageId == default(MessageId) ? MessageId.Generate() : messageId;
            var messageHeaders = new MessageHeaders
            {
                MessageId = myMessageId,
                Sent = DateTime.UtcNow,
                Origination = new Uri("http://localhost/platibus0"),
                Destination = new Uri("http://localhost/platibus1"),
                MessageName = messageName,
                ContentType = "text/plain"
            };
            var message = new Message(messageHeaders, content);
            await MessageJournal.Append(message, MessageJournalCategory.Sent);
            return message;
        }

        protected async Task<Message> GivenJournaledSentReply(string messageName, string content, MessageId relatedTo = default(MessageId))
        {
            var messageHeaders = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                Sent = DateTime.UtcNow,
                Origination = new Uri("http://localhost/platibus1"),
                Destination = new Uri("http://localhost/platibus0"),
                RelatedTo = relatedTo,
                MessageName = messageName,
                ContentType = "text/plain"
            };
            var message = new Message(messageHeaders, content);
            await MessageJournal.Append(message, MessageJournalCategory.Sent);
            return message;
        }

        protected async Task GivenJournaledReceivedMessage(string messageName, string content)
        {
            var messageHeaders = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                Sent = DateTime.UtcNow.AddSeconds(-1),
                Received = DateTime.UtcNow,
                Origination = new Uri("http://localhost/platibus0"),
                Destination = new Uri("http://localhost/platibus1"),
                MessageName = messageName,
                ContentType = "text/plain"
            };
            var message = new Message(messageHeaders, content);
            await MessageJournal.Append(message, MessageJournalCategory.Received);
        }

        protected async Task GivenJournaledReceivedReply(string messageName, string content, MessageId relatedTo = default(MessageId))
        {
            var messageHeaders = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                Sent = DateTime.UtcNow.AddSeconds(-1),
                Received = DateTime.UtcNow,
                Origination = new Uri("http://localhost/platibus1"),
                Destination = new Uri("http://localhost/platibus0"),
                RelatedTo = relatedTo,
                MessageName = messageName,
                ContentType = "text/plain"
            };
            var message = new Message(messageHeaders, content);
            await MessageJournal.Append(message, MessageJournalCategory.Received);
        }

        protected async Task GivenJournaledReceivedPublication(string messageName, string content, TopicName topic)
        {
            var published = DateTime.UtcNow.AddSeconds(-1);
            var messageHeaders = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                Sent = published,
                Published = published,
                Received = DateTime.UtcNow,
                Origination = new Uri("http://localhost/platibus1"),
                Destination = new Uri("http://localhost/platibus0"),
                Topic = topic,
                MessageName = messageName,
                ContentType = "text/plain"
            };
            var message = new Message(messageHeaders, content);
            await MessageJournal.Append(message, MessageJournalCategory.Received);
        }

        protected async Task GivenJournaledPublication(string messageName, string content, TopicName topic)
        {
            var messageHeaders = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                Published = DateTime.UtcNow,
                Origination = new Uri("http://localhost/platibus1"),
                MessageName = messageName,
                ContentType = "text/plain",
                Topic = topic
            };
            var message = new Message(messageHeaders, content);
            await MessageJournal.Append(message, MessageJournalCategory.Published);
        }
        
        protected void GivenFilter(Action<MessageJournalFilter> apply)
        {
            if (Filter == null)
            {
                Filter = new MessageJournalFilter();
            }
            apply(Filter);
        }

        protected async Task WhenReadingToEndOfJournal()
        {
            MessageJournalReadResult result;
            do
            {
                result = await MessageJournal.Read(Start, Count, Filter);
                Pages.Add(result);
                Start = result.Next;
            } while (!result.EndOfJournal);
        }

        protected void AssertCategory(IEnumerable<MessageJournalEntry> journaledMessages,
            MessageJournalCategory expectedCategory)
        {
            foreach (var journaledMessage in journaledMessages)
            {
                Assert.Equal(expectedCategory, journaledMessage.Category);
            }
        }

        protected void AssertTopic(IEnumerable<MessageJournalEntry> journaledMessages,
            TopicName expectedTopic)
        {
            foreach (var journaledMessage in journaledMessages)
            {
                Assert.Equal(expectedTopic, journaledMessage.Data.Headers.Topic);
            }
        }

        protected void AssertTimestampFrom(IEnumerable<MessageJournalEntry> journaledMessages,
            DateTime from)
        {
            foreach (var journaledMessage in journaledMessages)
            {
                Assert.True(from <= journaledMessage.Timestamp);
            }
        }

        protected void AssertOrigination(
            IEnumerable<MessageJournalEntry> journaledMessages,
            Uri expectedOrigination)
        {
            foreach (var journaledMessage in journaledMessages)
            {
                Assert.Equal(expectedOrigination, journaledMessage.Data.Headers.Origination);
            }
        }

        protected void AssertDestination(
            IEnumerable<MessageJournalEntry> journaledMessages,
            Uri expectedDestination)
        {
            foreach (var journaledMessage in journaledMessages)
            {
                Assert.Equal(expectedDestination, journaledMessage.Data.Headers.Destination);
            }
        }

        protected void AssertMessageName(
            IEnumerable<MessageJournalEntry> journaledMessages,
            string partial)
        {
            foreach (var journaledMessage in journaledMessages)
            {
                Assert.Contains(partial, journaledMessage.Data.Headers.MessageName);
            }
        }

        protected void AssertRelatedTo(
            IEnumerable<MessageJournalEntry> journaledMessages,
            MessageId expectedMessageId)
        {
            foreach (var journaledMessage in journaledMessages)
            {
                Assert.Equal(expectedMessageId, journaledMessage.Data.Headers.RelatedTo);
            }
        }

        protected void AssertSameResults(IList<MessageJournalReadResult> firstRun, IList<MessageJournalReadResult> secondRun)
        {
            Assert.Equal(firstRun.Count, secondRun.Count);
            for (var page = 0; page < firstRun.Count; page++)
            {
                var firstRunPage = firstRun[page];
                var secondRunPage = secondRun[page];

                Assert.Equal(firstRunPage.Start, secondRunPage.Start);
                Assert.Equal(firstRunPage.Next, secondRunPage.Next);
                Assert.Equal(firstRunPage.EndOfJournal, secondRunPage.EndOfJournal);
                Assert.Equal(firstRunPage.Entries.Count(), secondRunPage.Entries.Count());

                using (var firstRunMessages = firstRunPage.Entries.GetEnumerator())
                using (var secondRunMessages = secondRunPage.Entries.GetEnumerator())
                {
                    while (firstRunMessages.MoveNext() && secondRunMessages.MoveNext())
                    {
                        var firstRunMessage = firstRunMessages.Current;
                        var secondRunMessage = secondRunMessages.Current;

                        Assert.Equal(firstRunMessage.Position, secondRunMessage.Position);
                        Assert.Equal(firstRunMessage.Category, secondRunMessage.Category);
                        Assert.Equal(firstRunMessage.Data, secondRunMessage.Data, new MessageEqualityComparer());
                    }
                }
            }
        }
    }
}