// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
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
using System.Threading;
using System.Threading.Tasks;
using Pluribus.Serialization;

namespace Pluribus
{
    public static class SentMessageExtensions
    {
        public static async Task<Message> GetReply(this ISentMessage sentMessage, TimeSpan timeout = default(TimeSpan))
        {
            Message reply = null;
            var replyReceivedEvent = new ManualResetEvent(false);
            var subscription = sentMessage.ObserveReplies().Subscribe(r =>
            {
                reply = r;
                // ReSharper disable once AccessToDisposedClosure
                replyReceivedEvent.Set();
            });

            await replyReceivedEvent.WaitOneAsync(timeout).ConfigureAwait(false);

            subscription.Dispose();
            replyReceivedEvent.Dispose();

            return reply;
        }

        public static async Task<TContent> GetReplyContent<TContent>(this ISentMessage sentMessage,
            TimeSpan timeout = default(TimeSpan), ISerializationService serializationService = null)
        {
            if (serializationService == null)
            {
                serializationService = new DefaultSerializationService();
            }
            var message = await sentMessage.GetReply(timeout).ConfigureAwait(false);
            var contentType = message.Headers.ContentType;
            var serializer = serializationService.GetSerializer(contentType);
            var deserializedMessageContent = serializer.Deserialize<TContent>(message.Content);
            return deserializedMessageContent;
        }
    }
}