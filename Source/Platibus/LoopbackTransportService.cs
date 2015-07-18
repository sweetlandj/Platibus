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
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus
{
    /// <summary>
    /// Transport service that sends all messages to the sender.
    /// </summary>
    /// <remarks>
    /// Useful for in-process message passing within a single application.
    /// </remarks>
    public class LoopbackTransportService : ITransportService
    {
        private readonly Func<Message, IPrincipal, Task> _accept;

        public LoopbackTransportService(Func<Message, IPrincipal, Task> accept)
        {
            if (accept == null) throw new ArgumentNullException("accept");
            _accept = accept;
        }

        public async Task SendMessage(Message message, IEndpointCredentials credentials = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            await Task.Run(() => _accept(message, Thread.CurrentPrincipal), cancellationToken);
        }

        public async Task PublishMessage(Message message, TopicName topicName, CancellationToken cancellationToken)
        {
            await Task.Run(() => _accept(message, Thread.CurrentPrincipal), cancellationToken);
        }

        public Task Subscribe(IEndpoint endpoint, TopicName topicName, TimeSpan ttl, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(false);
        }
    }
}