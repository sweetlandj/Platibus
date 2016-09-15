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
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace Platibus.Http
{
    internal class OutboundQueueListener : IQueueListener
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Http);

        private readonly ITransportService _transportService;
        private readonly IMessageJournalingService _messageJournalingService;
        private readonly Func<Uri, IEndpointCredentials> _credentialsFactory;

        public OutboundQueueListener(ITransportService transportService,
            IMessageJournalingService messageJournalingService, Func<Uri, IEndpointCredentials> credentialsFactory)
        {
            if (transportService == null)
            {
                throw new ArgumentNullException("transportService");
            }
            _transportService = transportService;
            _messageJournalingService = messageJournalingService;
            _credentialsFactory = credentialsFactory ??
                                  (uri => null);
        }

        public async Task MessageReceived(Message message, IQueuedMessageContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (message.Headers.Expires < DateTime.UtcNow)
            {
                Log.WarnFormat("Discarding expired \"{0}\" message (ID {1}, expired {2})",
                    message.Headers.MessageName, message.Headers.MessageId, message.Headers.Expires);

                await context.Acknowledge();
                return;
            }

            var credentials = _credentialsFactory(message.Headers.Destination);

            await _transportService.SendMessage(message, credentials, cancellationToken);
            await context.Acknowledge();

            if (_messageJournalingService != null)
            {
                await _messageJournalingService.MessageSent(message, cancellationToken);
            }
        }
    }
}
