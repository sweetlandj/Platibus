// The MIT License (MIT)
// 
// Copyright (c) 2015 Jesse Sweetland
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
using System.Collections.Concurrent;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.InMemory
{
    /// <summary>
    /// A <see cref="IMessageQueueingService"/> implementation based on in-memory
    /// queues
    /// </summary>
    public class InMemoryMessageQueueingService : IMessageQueueingService
    {
        private readonly ConcurrentDictionary<QueueName, InMemoryQueue> _queues =
            new ConcurrentDictionary<QueueName, InMemoryQueue>();

        /// <summary>
        /// Establishes a named queue
        /// </summary>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="listener">An object that will receive messages off of the queue for processing</param>
        /// <param name="options">(Optional) Options that govern how the queue behaves</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used
        /// by the caller to cancel queue creation if necessary</param>
        /// <returns>Returns a task that will complete when the queue has been created</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queueName"/> or
        /// <paramref name="listener"/> is <c>null</c></exception>
        /// <exception cref="QueueAlreadyExistsException">Thrown if a queue with the specified
        /// name already exists</exception>
        public Task CreateQueue(QueueName queueName, IQueueListener listener, QueueOptions options = default(QueueOptions), CancellationToken cancellationToken = default(CancellationToken))
        {
            var queue = new InMemoryQueue(listener, options);
            if (!_queues.TryAdd(queueName, queue))
            {
                throw new QueueAlreadyExistsException(queueName);
            }
            queue.Init();
            return Task.FromResult(true);
        }

        /// <summary>
        /// Enqueues a message on a queue
        /// </summary>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="message">The message to enqueue</param>
        /// <param name="senderPrincipal">(Optional) The sender principal, if applicable</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be
        /// used be the caller to cancel the enqueue operation if necessary</param>
        /// <returns>Returns a task that will complete when the message has been enqueued</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queueName"/>
        /// or <paramref name="message"/> is <c>null</c></exception>
        public Task EnqueueMessage(QueueName queueName, Message message, IPrincipal senderPrincipal, CancellationToken cancellationToken = default(CancellationToken))
        {
            InMemoryQueue queue;
            if (!_queues.TryGetValue(queueName, out queue))
            {
                throw new QueueNotFoundException(queueName);
            }
            return queue.Enqueue(message, senderPrincipal);
        }
    }
}