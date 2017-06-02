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
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Queueing;

namespace Platibus.InMemory
{
    /// <summary>
    /// A message queues whose contents are maintained in memory and will be discarded when the
    /// application terminates
    /// </summary>
    public class InMemoryMessageQueue : AbstractMessageQueue
    {
        /// <summary>
        /// Creates a new <see cref="InMemoryMessageQueue"/>
        /// </summary>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="listener">The object that will be notified when messages are
        ///     added to the queue</param>
        /// <param name="options">(Optional) Settings that influence how the queue behaves</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queueName"/>, or 
        /// <paramref name="listener"/> are <c>null</c></exception>
        public InMemoryMessageQueue(QueueName queueName, IQueueListener listener, QueueOptions options = null) 
            : base(queueName, listener, options)
        {
        }

        /// <inheritdoc />
        protected override Task<IEnumerable<QueuedMessage>> SelectPendingMessages(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(Enumerable.Empty<QueuedMessage>());
        }

        /// <inheritdoc />
        protected override Task<QueuedMessage> InsertQueuedMessage(Message message, IPrincipal principal, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(new QueuedMessage(message, principal, 0));
        }

        /// <inheritdoc />
        protected override Task UpdateQueuedMessage(QueuedMessage queuedMessage, DateTime? acknowledged, DateTime? abandoned, int attempts, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(0);
        }
    }
}