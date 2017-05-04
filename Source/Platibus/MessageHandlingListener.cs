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
using Platibus.Serialization;

namespace Platibus
{
    internal class MessageHandlingListener : IQueueListener
    {
        private readonly Bus _bus;
        private readonly IMessageNamingService _messageNamingService;
        private readonly ISerializationService _serializationService;
        private readonly IEnumerable<IMessageHandler> _messageHandlers;

        public MessageHandlingListener(Bus bus, IMessageNamingService namingService,
            ISerializationService serializationService, IEnumerable<IMessageHandler> messageHandlers)
        {
            if (bus == null) throw new ArgumentNullException("bus");
            if (namingService == null) throw new ArgumentNullException("namingService");
            if (serializationService == null) throw new ArgumentNullException("serializationService");
            if (messageHandlers == null) throw new ArgumentNullException("messageHandlers");

            var handlerList = messageHandlers.Where(h => h != null).ToList();
            if (!handlerList.Any()) throw new ArgumentNullException("messageHandlers");

            _bus = bus;
            _messageNamingService = namingService;
            _serializationService = serializationService;
            _messageHandlers = handlerList;
        }

        public async Task MessageReceived(Message message, IQueuedMessageContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var messageContext = new BusMessageContext(_bus, context.Headers, context.Principal);
            await MessageHandler.HandleMessage(_messageNamingService, _serializationService, _messageHandlers,
                message, messageContext, cancellationToken);

            if (messageContext.MessageAcknowledged)
            {
                await context.Acknowledge();
            }
        }
    }
}