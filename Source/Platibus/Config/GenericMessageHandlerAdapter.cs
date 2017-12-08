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

namespace Platibus.Config
{
    /// <summary>
    /// Static factory for creating new instances of 
    /// <see cref="GenericMessageHandlerAdapter{TContent}"/>.
    /// </summary>
    public static class GenericMessageHandlerAdapter
    {
        /// <summary>
        /// Creates a new <see cref="GenericMessageHandlerAdapter{TContent}"/> instance
        /// that wraps the specified <paramref name="genericContent"/>, adapting it
        /// to the parameterless <see cref="GenericMessageHandlerAdapter{TContent}"/> interface.
        /// </summary>
        /// <typeparam name="TContent">The type of content handled</typeparam>
        /// <param name="genericContent">A generic type handler targeting messages
        /// with specific types of <typeparamref name="TContent"/></param>
        /// <returns>Returns a new <see cref="GenericMessageHandlerAdapter{TContent}"/> instance
        /// that wraps the specified <paramref name="genericContent"/></returns>
        public static GenericMessageHandlerAdapter<TContent> For<TContent>(IMessageHandler<TContent> genericContent)
        {
            return new GenericMessageHandlerAdapter<TContent>(genericContent);
        }
    }

    /// <summary>
    /// An <see cref="IMessageHandler"/> implementation that wraps a typed
    /// <see cref="IMessageHandler{TMessage}"/> instance.
    /// </summary>
    /// <typeparam name="TContent">The type of message content handled
    /// by the wrapped message handler.</typeparam>
    public class GenericMessageHandlerAdapter<TContent> : IMessageHandler
    {
        private readonly IMessageHandler<TContent> _genericMessageHandler;

        /// <summary>
        /// Initializes a new <see cref="GenericMessageHandlerAdapter{TContent}"/>
        /// wrapping the supplied <paramref name="genericMessageHandler"/>.
        /// </summary>
        /// <param name="genericMessageHandler">The generic message handler to wrap</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="genericMessageHandler"/>
        /// is <c>null</c></exception>
        public GenericMessageHandlerAdapter(IMessageHandler<TContent> genericMessageHandler)
        {
            _genericMessageHandler = genericMessageHandler ?? throw new ArgumentNullException(nameof(genericMessageHandler));
        }

        /// <summary>
        /// Handles (processes) an incoming message.
        /// </summary>
        /// <param name="content">The deserialied content (body) of the message</param>
        /// <param name="messageContext">The context in which the message was received
        /// including message metadata</param>
        /// <param name="cancellationToken">A cancellation token used by the host to
        /// indicate that message handling should stop as soon as feasible.</param>
        /// <returns>Returns a task that completes when the message has been handled</returns>
        public Task HandleMessage(object content, IMessageContext messageContext, CancellationToken cancellationToken)
        {
            return _genericMessageHandler.HandleMessage((TContent)content, messageContext, cancellationToken);
        }
    }
}
