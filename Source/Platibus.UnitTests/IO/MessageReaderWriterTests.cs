using System;
using System.IO;
using System.Threading.Tasks;
using Platibus.IO;
using Xunit;

namespace Platibus.UnitTests.IO
{
    public class MessageReaderWriterTests
    {
        protected Message Message;
        protected string MessageWriterOutput;
        protected Message MessageReaderOutput;

        [Fact]
        public async Task MessageCanBeReadWhenHeaderAndContentAreWrittenTogether()
        {
            GivenMessage();
            await WhenHeadersAndContentAreWrittenTogether();
            await AssertHeadersAndContentCanBeReadTogether();
            AssertMessageReaderOutputMatchesOriginalMessage();
        }

        [Fact]
        public async Task LegacyMessageCanBeRead()
        {
            GivenMessage();
            await WhenLegacyMessageIsWritten();
            await AssertHeadersAndContentCanBeReadTogether();
            AssertMessageReaderOutputMatchesOriginalMessage();
        }

        [Fact]
        public async Task MessageCanBeReadWhenHeaderAndContentAreWrittenSeparately()
        {
            GivenMessage();
            await WhenHeadersAndContentAreWrittenSeparately();
            await AssertHeadersAndContentCanBeReadTogether();
            AssertMessageReaderOutputMatchesOriginalMessage();
        }

        [Fact]
        public async Task HeaderAndContentCanBeReadSeparately()
        {
            GivenMessage();
            await WhenHeadersAndContentAreWrittenTogether();
            await AssertHeadersAndContentCanBeReadSeparately();
            AssertMessageReaderOutputMatchesOriginalMessage();
        }

        protected Message GivenMessage()
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

        protected async Task WhenHeadersAndContentAreWrittenSeparately()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var messageWriter = new MessageWriter(stringWriter))
                {
                    await messageWriter.WriteMessageHeaders(Message.Headers);
                    await messageWriter.WriteMessageContent(Message.Content);
                }
                MessageWriterOutput = stringWriter.ToString();
            }
        }

        protected async Task WhenHeadersAndContentAreWrittenTogether()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var messageWriter = new MessageWriter(stringWriter))
                {
                    await messageWriter.WriteMessage(Message);
                }
                MessageWriterOutput = stringWriter.ToString();
            }
        }

        protected async Task WhenLegacyMessageIsWritten()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var messageWriter = new LegacyMessageWriter(stringWriter))
                {
                    await messageWriter.WritePrincipal(null);
                    await messageWriter.WriteMessage(Message);
                }
                MessageWriterOutput = stringWriter.ToString();
            }
        }

        protected async Task AssertHeadersAndContentCanBeReadSeparately()
        {
            using (var stringReader = new StringReader(MessageWriterOutput))
            using (var messageReader = new MessageReader(stringReader))
            {
                var headers = await messageReader.ReadMessageHeaders();
                var content = await messageReader.ReadMessageContent();
                MessageReaderOutput = new Message(headers, content);
            }
        }

        protected async Task AssertHeadersAndContentCanBeReadTogether()
        {
            using (var stringReader = new StringReader(MessageWriterOutput))
            using (var messageReader = new MessageReader(stringReader))
            {
                MessageReaderOutput = await messageReader.ReadMessage();
            }

            AssertMessageReaderOutputMatchesOriginalMessage();
        }

        protected void AssertMessageReaderOutputMatchesOriginalMessage()
        {
            Assert.Equal(Message, MessageReaderOutput, new MessageEqualityComparer());
        }
    }
}
