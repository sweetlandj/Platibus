// The MIT License (MIT)
// 
// Copyright (c) 2018 Jesse Sweetland
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

using System.Collections.Concurrent;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Queueing
{
    /// <inheritdoc />
    /// <summary>
    /// A message queueing service that does not actually queue messages.
    /// </summary>
    /// <remarks>
    /// The task returned by
    /// <see cref="EnqueueMessage"/> will not complete until all handlers associated with the
    /// queue have been invoked.  Messages will not be retried; any exceptions that occur
    /// (including acknowledgement failures) will immediately propagate to the caller in the
    /// form of an <see cref="System.AggregateException"/>.
    /// </remarks>
    public class VirtualMessageQueueingService : IMessageQueueingService
    {
        private readonly ConcurrentDictionary<QueueName, VirtualQueue> _virtualQueues = new ConcurrentDictionary<QueueName, VirtualQueue>();
        
        /// <inheritdoc />
        public Task CreateQueue(QueueName queueName, IQueueListener listener, QueueOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var myOptions = options ?? new QueueOptions();
            _virtualQueues.GetOrAdd(queueName, new VirtualQueue(listener, myOptions.AutoAcknowledge));
            return Task.FromResult(0);
        }

        /// <inheritdoc />
        public async Task EnqueueMessage(QueueName queueName, Message message, IPrincipal senderPrincipal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!_virtualQueues.TryGetValue(queueName, out var queue))
            {
                throw new QueueNotFoundException(queueName);
            }

            var context = new QueuedMessageContext(message, senderPrincipal);
            await queue.Listener.MessageReceived(message, context, cancellationToken);
            var acknowledged = context.Acknowledged || queue.AutoAcknowledge;
            if (!acknowledged)
            {
                throw new MessageNotAcknowledgedException();
            }
        }

        private class VirtualQueue
        {
            public IQueueListener Listener { get; }

            public bool AutoAcknowledge { get; }

            public VirtualQueue(IQueueListener listener, bool autoAcknowledge)
            {
                AutoAcknowledge = autoAcknowledge;
                Listener = listener;
            }
        }
    }
}
