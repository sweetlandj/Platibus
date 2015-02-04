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

using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Pluribus
{
    public interface IMessageContext
    {
        /// <summary>
        ///     A reference to the bus instance
        /// </summary>
        IBus Bus { get; }

        /// <summary>
        ///     The headers on the incoming message
        /// </summary>
        IMessageHeaders Headers { get; }

        /// <summary>
        ///     The identity of the sender
        /// </summary>
        IPrincipal SenderPrincipal { get; }

        /// <summary>
        ///     Acknowledge receipt of the message, indicating that the message has been successfully handled.
        /// </summary>
        void Acknowledge();

        /// <summary>
        ///     Send a reply to the message.  This method can be called more than once.
        /// </summary>
        /// <param name="reply">The reply</param>
        /// <param name="options">Options for sending the reply.</param>
        /// <param name="cancellationToken">A cancellation token for the send task</param>
        Task SendReply(object reply, SendOptions options = default(SendOptions),
            CancellationToken cancellationToken = default(CancellationToken));
    }
}