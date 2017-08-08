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
using Xunit;

namespace Platibus.UnitTests
{
    [Trait("Category", "UnitTests")]
    public class MessageHeadersTests
    {
        protected MessageHeaders MessageHeaders = new MessageHeaders();

        [Fact]
        public void DatesValuesCanBeSetAndRetrieved()
        {
            var expected = DateTime.UtcNow;
            MessageHeaders.SetDateTime("TestDate", expected);
            var actual = MessageHeaders.GetDateTime("TestDate");
            Assert.Equal(expected, actual);
            AssertSingleHeaderExists("TestDate");
        }

        [Fact]
        public void UriValuesCanBeSetAndRetrieved()
        {
            var expected = new Uri("http://user:pass@localhost:8080/platibus/");
            MessageHeaders.SetUri("TestUri", expected);
            var actual = MessageHeaders.GetUri("TestUri");
            Assert.Equal(expected, actual);
            AssertSingleHeaderExists("TestUri");
        }

        [Fact]
        public void IntValuesCanBeSetAndRetrieved()
        {
            var expected = new Random().Next();
            MessageHeaders.SetInt("TestInt", expected);
            var actual = MessageHeaders.GetInt("TestInt");
            Assert.Equal(expected, actual);
            AssertSingleHeaderExists("TestInt");
        }

        [Fact]
        public void MessageIdCanBeSetAndRetrieved()
        {
            var expected = MessageId.Generate();
            MessageHeaders.MessageId = expected;
            var actual = MessageHeaders.MessageId;
            Assert.Equal(expected, actual);
            AssertSingleHeaderExists(HeaderName.MessageId);
        }

        [Fact]
        public void MessageNameCanBeSetAndRetrieved()
        {
            var expected = new MessageName("test:Message");
            MessageHeaders.MessageName = expected;
            var actual = MessageHeaders.MessageName;
            Assert.Equal(expected, actual);
            AssertSingleHeaderExists(HeaderName.MessageName);
        }

        [Fact]
        public void ExpiresCanBeSetAndRetrieved()
        {
            var expected = DateTime.UtcNow.AddMinutes(5);
            MessageHeaders.Expires = expected;
            var actual = MessageHeaders.Expires;
            Assert.Equal(expected, actual);
            AssertSingleHeaderExists(HeaderName.Expires);
        }

        [Fact]
        public void ExpiresDefaultsToMaxDate()
        {
            var expected = DateTime.MaxValue;
            var actual = MessageHeaders.Expires;
            Assert.Equal(expected, actual);
            Assert.Empty(MessageHeaders);
        }

        [Fact]
        public void DestinationCanBeSetAndRetrieved()
        {
            var expected = new Uri("http://localhost/platibus/");
            MessageHeaders.Destination = expected;
            var actual = MessageHeaders.Destination;
            Assert.Equal(expected, actual);
            AssertSingleHeaderExists(HeaderName.Destination);
        }

        [Fact]
        public void OriginationCanBeSetAndRetrieved()
        {
            var expected = new Uri("http://localhost/platibus/");
            MessageHeaders.Origination = expected;
            var actual = MessageHeaders.Origination;
            Assert.Equal(expected, actual);
            AssertSingleHeaderExists(HeaderName.Origination);
        }

        [Fact]
        public void ReplyToCanBeSetAndRetrieved()
        {
            var expected = new Uri("http://localhost/platibus/");
            MessageHeaders.ReplyTo = expected;
            var actual = MessageHeaders.ReplyTo;
            Assert.Equal(expected, actual);
            AssertSingleHeaderExists(HeaderName.ReplyTo);
        }

        [Fact]
        public void ImportanceCanBeSetAndRetrieved()
        {
            var expected = MessageImportance.Critical;
            MessageHeaders.Importance = expected;
            var actual = MessageHeaders.Importance;
            Assert.Equal(expected, actual);
            AssertSingleHeaderExists(HeaderName.Importance);
        }

        [Fact]
        public void SentCanBeSetAndRetrieved()
        {
            var expected = DateTime.UtcNow;
            MessageHeaders.Sent = expected;
            var actual = MessageHeaders.Sent;
            Assert.Equal(expected, actual);
            AssertSingleHeaderExists(HeaderName.Sent);
        }

        [Fact]
        public void ReceivedCanBeSetAndRetrieved()
        {
            var expected = DateTime.UtcNow;
            MessageHeaders.Received = expected;
            var actual = MessageHeaders.Received;
            Assert.Equal(expected, actual);
            AssertSingleHeaderExists(HeaderName.Received);
        }

        [Fact]
        public void PublishedCanBeSetAndRetrieved()
        {
            var expected = DateTime.UtcNow;
            MessageHeaders.Published = expected;
            var actual = MessageHeaders.Published;
            Assert.Equal(expected, actual);
            AssertSingleHeaderExists(HeaderName.Published);
        }

        [Fact]
        public void ContentTypeCanBeSetAndRetrieved()
        {
            var expected = "application/json";
            MessageHeaders.ContentType = expected;
            var actual = MessageHeaders.ContentType;
            Assert.Equal(expected, actual);
            AssertSingleHeaderExists(HeaderName.ContentType);
        }

        [Fact]
        public void RelatedToCanBeSetAndRetrieved()
        {
            var expected = MessageId.Generate();
            MessageHeaders.RelatedTo = expected;
            var actual = MessageHeaders.RelatedTo;
            Assert.Equal(expected, actual);
            AssertSingleHeaderExists(HeaderName.RelatedTo);
        }

        [Fact]
        public void TopicCanBeSetAndRetrieved()
        {
            var expected = new TopicName("TestEvents");
            MessageHeaders.Topic = expected;
            var actual = MessageHeaders.Topic;
            Assert.Equal(expected, actual);
            AssertSingleHeaderExists(HeaderName.Topic);
        }

        protected virtual void AssertSingleHeaderExists(HeaderName headerName)
        {
            Assert.Equal(1, MessageHeaders.Count());
            Assert.Equal(headerName, MessageHeaders.Select(h => h.Key).FirstOrDefault());
        }
    }
}
