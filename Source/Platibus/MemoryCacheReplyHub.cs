// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
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
using Platibus.Serialization;
#if NET452 || NET461
using System.Runtime.Caching;
#endif
#if NETSTANDARD2_0
using Microsoft.Extensions.Caching.Memory;
#endif

namespace Platibus
{
    /// <inheritdoc />
    /// <summary>
    /// Uses a memory cache to store sent messages and route related messages to their
    /// reply stream
    /// </summary>
    public class MemoryCacheReplyHub : IDisposable
    {
        private readonly MessageMarshaller _messageMarshaller;
        private readonly IDiagnosticService _diagnosticService;
#if NET452 || NET461
        private readonly MemoryCache _cache = new MemoryCache("MemoryCacheReplyHub");
#endif
#if NETSTANDARD2_0
        private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
#endif
        private readonly TimeSpan _replyTimeout;
        private bool _disposed;

        /// <summary>
        /// Creates a new <see cref="MemoryCacheReplyHub"/> that will hold sent messages
        /// in memory until the specified <paramref name="replyTimeout"/> has elapsed
        /// </summary>
        /// <param name="messageMarshaller">A service used to deserialize message content</param>
        /// <param name="diagnosticService">A service through which diagnostic events related
        /// to handling replies will be reported</param>
        /// <param name="replyTimeout">The maximum amount of time to hold send messages
        ///     in memory before they are evicted from cache</param>
        public MemoryCacheReplyHub(MessageMarshaller messageMarshaller, IDiagnosticService diagnosticService, TimeSpan replyTimeout)
        {
            _messageMarshaller = messageMarshaller ?? throw new ArgumentNullException(nameof(messageMarshaller));
            _diagnosticService = diagnosticService ?? throw new ArgumentNullException(nameof(diagnosticService));
            _replyTimeout = replyTimeout <= TimeSpan.Zero ? TimeSpan.FromMinutes(5) : replyTimeout;
        }

        /// <summary>
        /// Creates a <see cref="ISentMessage"/> wrapper for the specified 
        /// <paramref name="message"/> and stores it in cache
        /// </summary>
        /// <param name="message">The recently sent message</param>
        /// <returns>Returns a <see cref="ISentMessage"/> that can be used to listen
        /// for replies</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/>
        /// is <c>null</c></exception>
        public ISentMessage CreateSentMessage(Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            CheckDisposed();

            var messageId = message.Headers.MessageId;
            var replyStreamExpiration = DateTime.UtcNow.Add(_replyTimeout);
#if NET452 || NET461
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
#endif
#if NETSTANDARD2_0
            var replyStream = _cache.GetOrCreate(messageId, entry =>
            {
                entry.AbsoluteExpiration = replyStreamExpiration;
                return new ReplyStream();
            });
            return new SentMessageWithCachedReplies(messageId, replyStream);
#endif
        }

        /// <summary>
        /// Called by the bus to indicate that a message has been received that is
        /// related to another message (possibly a reply)
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Returns a task that will complete when all reply stream
        /// observers have been notified that a reply was received</returns>
        public Task NotifyReplyReceived(Message message)
        {
            CheckDisposed();
            var relatedTo = message.Headers.RelatedTo;
            return Task.Run(() =>
            {
                if (_cache.Get(relatedTo) is ReplyStream replyStream)
                {
                    var messageContent = _messageMarshaller.Unmarshal(message);
                    replyStream.NotifyReplyReceived(messageContent);
                }
                else
                {
                    _diagnosticService.Emit(
                        new DiagnosticEventBuilder(this, DiagnosticEventType.CorrelationFailed)
                        {
                            Message = message,
                            Detail = $"No sent message found with ID {relatedTo}"
                        }.Build());
                }
            });
        }

        /// <summary>
        /// Called by the bus to indicate that the last message related to the
        /// specified message ID has been received and the reply stream can be
        /// completed
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Returns a task when all reply stream observers have been 
        /// notified that the last reply was received</returns>
        public Task NotifyLastReplyReceived(Message message)
        {
            CheckDisposed();
            var relatedTo = message.Headers.RelatedTo;
            return Task.Run(() =>
            {
#if NET452 || NET461
                var replyStream = _cache.Remove(relatedTo) as ReplyStream;
                replyStream?.NotifyCompleted();
#endif
#if NETSTANDARD2_0
                var replyStream = _cache.Get<ReplyStream>(relatedTo);
                replyStream?.NotifyCompleted();
                _cache.Remove(message);
#endif
            });
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        /// <summary>
        /// Finalizer that ensures the memory cache disposed when this object goes
        /// out of scope
        /// </summary>
        ~MemoryCacheReplyHub()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called by the <see cref="Dispose()"/> method or finalizer to ensure that
        /// resources are released
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the 
        /// <see cref="Dispose()"/> method (<c>true</c>) or the finalizer (<c>false</c>)</param>
        /// <remarks>
        /// This method will not be called more than once
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
				_cache.Dispose();
            }
        }
    }
}