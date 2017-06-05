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
using Xunit;

namespace Platibus.UnitTests
{
    [Trait("Category", "UnitTests")]
    internal class SentMessageExtensionsTests
    {
        [Fact]
        public async Task FirstReplyIsReturned()
        {
            var memoryCacheReplyHub = new MemoryCacheReplyHub(TimeSpan.FromSeconds(3));
            var messageId = MessageId.Generate();
            var message = new Message(new MessageHeaders { MessageId = messageId }, "Hello, world!");
            var sentMessage = memoryCacheReplyHub.CreateSentMessage(message);

            const string reply = "Hello yourself!";
            var replyTask = Task.Delay(TimeSpan.FromMilliseconds(500))
                .ContinueWith(t => memoryCacheReplyHub.ReplyReceived(reply, messageId));

            var awaitedReply = await sentMessage.GetReply(TimeSpan.FromSeconds(3));
            await replyTask;

            Assert.NotNull(awaitedReply);
            Assert.Equal(reply, awaitedReply);
        }
    }
}