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

namespace Platibus.IntegrationTests
{
    [Trait("Category", "IntegrationTests")]
    public abstract class SendAndReplyTests
    {
        private static readonly Random RNG = new Random();

        protected readonly IBus Sender;
        protected readonly IBus Receiver;

        protected TestMessage Message;
        protected SendOptions SendOptions;
        protected ISentMessage SentMessage;

        protected SendAndReplyTests(IBus sender, IBus receiver)
        {
            Sender = sender;
            Receiver = receiver;
        }
        
        [Fact]
        public async Task SenderCanReceiveReplies()
        {
            GivenTestMessage();
            await WhenMessageSent();
            await AssertReplyReceived();
        }
        
        protected void GivenTestMessage()
        {
            Message = new TestMessage
            {
                GuidData = Guid.NewGuid(),
                IntData = RNG.Next(0, int.MaxValue),
                StringData = "Hello, world!",
                DateData = DateTime.UtcNow
            };
        }

        protected void GivenNotAuthorizedToSend()
        {
            Message.SimulateAuthorizationFailure = true;
        }

        protected void GivenMessageNotAcknowledged()
        {
            Message.SimulateAcknowledgementFailure = true;
        }

        protected async Task WhenMessageSent()
        {
            SentMessage = await Sender.Send(Message, new SendOptions {Synchronous = true});
        }

        protected async Task AssertReplyReceived()
        {
            Assert.NotNull(SentMessage);

            var timeout = TimeSpan.FromSeconds(5);
            var reply = await SentMessage.GetReply(timeout);
            Assert.NotNull(reply);
            Assert.IsType<TestReply>(reply);

            var testReply = (TestReply) reply;
            Assert.Equal(Message.GuidData, testReply.GuidData);
            Assert.Equal(Message.IntData, testReply.IntData);
            Assert.Equal(Message.StringData, testReply.StringData);
            Assert.Equal(Message.DateData, testReply.DateData);
        }
    }
}
