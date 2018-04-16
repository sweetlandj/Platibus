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
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Journaling;

namespace Platibus
{
    /// <inheritdoc />
    /// <summary>
    /// Transport service that sends all messages back to the sender
    /// </summary>
    /// <remarks>
    /// Useful for in-process message passing within a single application
    /// </remarks>
    public class LoopbackTransportService : ITransportService
    {
        private readonly IMessageJournal _messageJournal;

        /// <inheritdoc />
        /// <summary>
        /// Initialies a new <see cref="T:Platibus.LoopbackTransportService" />
        /// </summary>
        public LoopbackTransportService() : this(null)
        {
        }

        /// <summary>
        /// Initialies a new <see cref="LoopbackTransportService"/> with the specified
        /// <paramref name="messageJournal"/>
        /// </summary>
        /// <param name="messageJournal">(Optional) The journal to which copies of sent, received, and/or
        /// published messages will be recorded</param>
        public LoopbackTransportService(IMessageJournal messageJournal)
        {
            _messageJournal = messageJournal;
        }

        /// <summary>
        /// Event raised when a message is received
        /// </summary>
        public event TransportMessageEventHandler MessageReceived;

        /// <inheritdoc />
        public async Task SendMessage(Message message, IEndpointCredentials credentials = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (_messageJournal != null)
            {
                await _messageJournal.Append(message, MessageJournalCategory.Sent, cancellationToken);
            }

            await ReceiveMessage(message, Thread.CurrentPrincipal, cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task PublishMessage(Message message, TopicName topicName, CancellationToken cancellationToken)
        { 
            if (_messageJournal != null)
            {
                await _messageJournal.Append(message, MessageJournalCategory.Published, cancellationToken);
            }

            await ReceiveMessage(message, Thread.CurrentPrincipal, cancellationToken);
        }
        
        /// <inheritdoc />
        public Task Subscribe(IEndpoint endpoint, TopicName topicName, TimeSpan ttl, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public async Task ReceiveMessage(Message message, IPrincipal principal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var messageReceivedHandlers = MessageReceived;
            if (messageReceivedHandlers != null)
            {
                if (_messageJournal != null)
                {
                    await _messageJournal.Append(message, MessageJournalCategory.Received, cancellationToken);
                }

                var args = new TransportMessageEventArgs(message, Thread.CurrentPrincipal, cancellationToken);
                await messageReceivedHandlers(this, args);
            }
        }
    }
}