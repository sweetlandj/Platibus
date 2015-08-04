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

using System.Threading;
using System.Threading.Tasks;

namespace Platibus
{
    /// <summary>
    /// An interface that describes an object that handles (processes) incoming
    /// messages.
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Handles (processes) an incoming message.
        /// </summary>
        /// <param name="content">The deserialied content (body) of the message</param>
        /// <param name="messageContext">The context in which the message was received
        /// including message metadata</param>
        /// <param name="cancellationToken">A cancellation token used by the host to
        /// indicate that message handling should stop as soon as feasible.</param>
        /// <returns>Returns a task that completes when the message has been handled</returns>
        Task HandleMessage(object content, IMessageContext messageContext, CancellationToken cancellationToken);
    }

    /// <summary>
    /// An interface that describes an object that handles (processes) incoming
    /// messages with a specific type of deserialized content.
    /// </summary>
    /// <typeparam name="TContent">The type of content expected</typeparam>
    public interface IMessageHandler<in TContent>
    {
        /// <summary>
        /// Handles (processes) an incoming message.
        /// </summary>
        /// <param name="content">The deserialied content (body) of the message</param>
        /// <param name="messageContext">The context in which the message was received
        /// including message metadata</param>
        /// <param name="cancellationToken">A cancellation token used by the host to
        /// indicate that message handling should stop as soon as feasible.</param>
        /// <returns>Returns a task that completes when the message has been handled</returns>
        Task HandleMessage(TContent content, IMessageContext messageContext, CancellationToken cancellationToken);
    }
}