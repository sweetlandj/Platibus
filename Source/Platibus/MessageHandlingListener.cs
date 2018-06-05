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
using System.Threading;
using System.Threading.Tasks;

namespace Platibus
{
    /// <inheritdoc />
    /// <summary>
    /// A <see cref="T:Platibus.IQueueListener" /> that routes a queued message to one or more <see cref="T:Platibus.IMessageHandler" />s.
    /// </summary>
    internal class MessageHandlingListener : IQueueListener
    {
        private readonly Bus _bus;
        private readonly IEnumerable<IMessageHandler> _messageHandlers;
        private readonly MessageHandler _messageHandler;

        /// <summary>
        /// Initializes a new <see cref="MessageHandlingListener"/>
        /// </summary>
        /// <param name="bus">The bus instance to pass to the <paramref name="messageHandlers"/> via
        /// the <see cref="IMessageContext"/></param>
        /// <param name="messageHandler">Object responsible for unmarshalling <see cref="Message"/>s into strongly-typed
        /// content representations and routing them to the <paramref name="messageHandlers"/></param>
        /// <param name="messageHandlers">The handler(s) that will process the queued message</param>
        public MessageHandlingListener(Bus bus, MessageHandler messageHandler, IEnumerable<IMessageHandler> messageHandlers)
        {
            if (messageHandlers == null)
            {
                throw new ArgumentNullException(nameof(messageHandlers));
            }

            var handlerList = messageHandlers.Where(h => h != null).ToList();
            if (!handlerList.Any()) throw new ArgumentNullException(nameof(messageHandlers));

            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _messageHandlers = handlerList;
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
        }

        /// <inheritdoc />
        public async Task MessageReceived(Message message, IQueuedMessageContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var messageContext = new BusMessageContext(_bus, context.Headers, context.Principal);
            await _messageHandler.HandleMessage(_messageHandlers, message, messageContext, cancellationToken);
            if (messageContext.MessageAcknowledged)
            {
                await context.Acknowledge();
            }
        }
    }
}