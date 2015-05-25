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
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Platibus
{
    public class MemoryCacheReplyHub : IDisposable
    {
        private bool _disposed;
        private readonly MemoryCache _cache = new MemoryCache("MemoryCacheReplyHub");
        private readonly TimeSpan _replyTimeout;

        public MemoryCacheReplyHub(TimeSpan replyTimeout)
        {
            _replyTimeout = (replyTimeout <= TimeSpan.Zero) ? TimeSpan.FromMinutes(5) : replyTimeout;
        }

        public ISentMessage CreateSentMessage(Message message)
        {
            CheckDisposed();

            var messageId = message.Headers.MessageId;
            var replyStreamExpiration = DateTime.UtcNow.Add(_replyTimeout);
            var newReplyStream = new ReplyStream();
            var replyStream = (ReplyStream) _cache.AddOrGetExisting(messageId, newReplyStream, replyStreamExpiration);
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (replyStream == null)
            {
                // MemoryCache.AddOrGetExisting returns null if the key does not
                // already exist, so use the one we just created. See:
                // http://msdn.microsoft.com/en-us/library/dd988741%28v=vs.110%29.aspx
                replyStream = newReplyStream;
            }
            return new SentMessageWithCachedReplies(messageId, replyStream);
        }

        public Task ReplyReceived(object reply, MessageId relatedToMessageId)
        {
            CheckDisposed();
            return Task.Run(() =>
            {
                var replyStream = _cache.Get(relatedToMessageId) as ReplyStream;
                if (replyStream == null)
                {
                    return;
                }

                replyStream.NotifyReplyReceived(reply);
            });
        }

        public Task NotifyLastReplyReceived(MessageId relatedToMessageId)
        {
            return Task.Run(() =>
            {
                var replyStream = _cache.Remove(relatedToMessageId) as ReplyStream;
                if (replyStream == null)
                {
                    return;
                }

                replyStream.NotifyCompleted();
            });
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        ~MemoryCacheReplyHub()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cache.Dispose();
            }
        }
    }
}