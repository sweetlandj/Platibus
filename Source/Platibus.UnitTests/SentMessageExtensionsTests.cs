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

using Platibus.Diagnostics;
using Platibus.Serialization;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Platibus.UnitTests
{
    [Trait("Category", "UnitTests")]
    public class SentMessageExtensionsTests
    {
        protected CancellationTokenSource CancellationTokenSource;
        protected MemoryCacheReplyHub ReplyHub;
        protected Message Message;
        protected ISentMessage SentMessage;
        protected Message Reply;

        public SentMessageExtensionsTests()
        {
            var timeout = TimeSpan.FromSeconds(3);
            CancellationTokenSource = new CancellationTokenSource(timeout);

            var messageSerializationService = new MessageMarshaller();
            var diagnosticService = DiagnosticService.DefaultInstance;

            ReplyHub = new MemoryCacheReplyHub(
                messageSerializationService, 
                diagnosticService,
                timeout);
        }

        [Fact]
        public async Task GetReplyReturnsFirstReply()
        {
            GivenSentMessage();
            GivenReply();
            WhenReplyReceived();

            var cancellationToken = CancellationTokenSource.Token;
            var awaitedReplyContent = await SentMessage.GetReply(cancellationToken);

            Assert.NotNull(awaitedReplyContent);
            Assert.Equal(Reply.Content, awaitedReplyContent);
        }

        protected void GivenSentMessage()
        {
            Message = new Message(new MessageHeaders
            {
                MessageId = MessageId.Generate()
            }, "Hello, world!");

            SentMessage = ReplyHub.CreateSentMessage(Message);
        }

        protected void GivenReply()
        {
            Reply = new Message(new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                RelatedTo = Message?.Headers.MessageId ?? default(MessageId)
            }, "Hello, back!");
        }

        protected void WhenReplyReceived()
        {
            ReplyHub.NotifyReplyReceived(Reply);
        }
    }
}