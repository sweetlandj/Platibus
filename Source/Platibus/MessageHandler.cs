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
using Platibus.Diagnostics;
using Platibus.Serialization;

namespace Platibus
{
    internal class MessageHandler 
    {
        private readonly IMessageNamingService _messageNamingService;
        private readonly ISerializationService _serializationService;
        private readonly IDiagnosticService _diagnosticService;

        public MessageHandler(IMessageNamingService messageNamingService, ISerializationService serializationService, IDiagnosticService diagnosticService = null)
        {
            _messageNamingService = messageNamingService ?? throw new ArgumentNullException(nameof(messageNamingService));
            _serializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
            _diagnosticService = diagnosticService ?? DiagnosticService.DefaultInstance;
        }

        public async Task HandleMessage(IEnumerable<IMessageHandler> messageHandlers, Message message,
            IMessageContext messageContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (message.Headers.Expires < DateTime.UtcNow)
            {
                await _diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.MessageExpired)
                    {
                        Detail = "Discarding message that expired " + message.Headers.Expires,
                        Message = message
                    }.Build(), cancellationToken);
                
                messageContext.Acknowledge();
                return;
            }

            var messageType = _messageNamingService.GetTypeForName(message.Headers.MessageName);
            var serializer = _serializationService.GetSerializer(message.Headers.ContentType);
            var messageContent = serializer.Deserialize(message.Content, messageType);

            var handlingTasks = messageHandlers.Select(handler =>
                handler.HandleMessage(messageContent, messageContext, cancellationToken));

            await Task.WhenAll(handlingTasks);
        }
    }
}