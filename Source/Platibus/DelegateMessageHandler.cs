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

namespace Platibus
{
    /// <summary>
    /// An <see cref="IMessageHandler"/> implementation that wraps a function or
    /// action delegate to handle messages
    /// </summary>
    /// <remarks>
    /// This implementation enables developers to use simple lamba expressions to
    /// handle messages rather than creating many implementations of the 
    /// <see cref="IMessageHandler"/> interface.
    /// </remarks>
    public class DelegateMessageHandler : DelegateMessageHandler<object>
    {
        /// <summary>
        /// Initializes a new <see cref="DelegateMessageHandler"/> with the specified
        /// function delegate
        /// </summary>
        /// <param name="handleMessage">The function delegate that will handle the message</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="handleMessage"/> is
        /// <c>null</c></exception>
        public DelegateMessageHandler(Func<object, IMessageContext, Task> handleMessage)
            : base(handleMessage)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="DelegateMessageHandler"/> with the specified
        /// function delegate
        /// </summary>
        /// <param name="handleMessage">The function delegate that will handle the message</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="handleMessage"/> is
        /// <c>null</c></exception>
        public DelegateMessageHandler(Func<object, IMessageContext, CancellationToken, Task> handleMessage)
            : base(handleMessage)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="DelegateMessageHandler"/> with the specified
        /// action delegate
        /// </summary>
        /// <param name="handleMessage">The actiopn delegate that will handle the message</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="handleMessage"/> is
        /// <c>null</c></exception>
        public DelegateMessageHandler(Action<object, IMessageContext> handleMessage)
            : base(handleMessage)
        {
        }

        /// <summary>
        /// Factory method for creating new <see cref="DelegateMessageHandler{TContent}"/>
        /// instances for specific types of content
        /// </summary>
        /// <typeparam name="TContent">The type of content expected by the handler</typeparam>
        /// <param name="handleMessage">The function delegate for handling the message</param>
        /// <returns>Returns an instance of <see cref="DelegateMessageHandler{TContent}"/> that
        /// wraps the specified <paramref name="handleMessage"/> delegate</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="handleMessage"/> is
        /// <c>null</c></exception>
        public static DelegateMessageHandler<TContent> For<TContent>(Func<TContent, IMessageContext, Task> handleMessage)
        {
            return new DelegateMessageHandler<TContent>(handleMessage);
        }

        /// <summary>
        /// Factory method for creating new <see cref="DelegateMessageHandler{TContent}"/>
        /// instances for specific types of content
        /// </summary>
        /// <typeparam name="TContent">The type of content expected by the handler</typeparam>
        /// <param name="handleMessage">The function delegate for handling the message</param>
        /// <returns>Returns an instance of <see cref="DelegateMessageHandler{TContent}"/> that
        /// wraps the specified <paramref name="handleMessage"/> delegate</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="handleMessage"/> is
        /// <c>null</c></exception>
        public static DelegateMessageHandler<TContent> For<TContent>(Func<TContent, IMessageContext, CancellationToken, Task> handleMessage)
        {
            return new DelegateMessageHandler<TContent>(handleMessage);
        }

        /// <summary>
        /// Factory method for creating new <see cref="DelegateMessageHandler{TContent}"/>
        /// instances for specific types of content
        /// </summary>
        /// <typeparam name="TContent">The type of content expected by the handler</typeparam>
        /// <param name="handleMessage">The action delegate for handling the message</param>
        /// <returns>Returns an instance of <see cref="DelegateMessageHandler{TContent}"/> that
        /// wraps the specified <paramref name="handleMessage"/> delegate</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="handleMessage"/> is
        /// <c>null</c></exception>
        public static DelegateMessageHandler<TContent> For<TContent>(Action<TContent, IMessageContext> handleMessage)
        {
            return new DelegateMessageHandler<TContent>(handleMessage);
        }
    }

    /// <summary>
    /// An <see cref="IMessageHandler"/> implementation that wraps a function or
    /// action delegate to handle messages
    /// </summary>
    /// <typeparam name="TContent">The type of content expected by the handler</typeparam>
    /// <remarks>
    /// This implementation enables developers to use simple lamba expressions to
    /// handle messages rather than creating many implementations of the 
    /// <see cref="IMessageHandler"/> interface.
    /// </remarks>
    public class DelegateMessageHandler<TContent> : IMessageHandler
    {
        private readonly Func<TContent, IMessageContext, CancellationToken, Task> _handleMessage;

        /// <summary>
        /// Initializes a new <see cref="DelegateMessageHandler"/> with the specified
        /// function delegate
        /// </summary>
        /// <param name="handleMessage">The function delegate that will handle the message</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="handleMessage"/> is
        /// <c>null</c></exception>
        public DelegateMessageHandler(Func<TContent, IMessageContext, Task> handleMessage)
        {
            if (handleMessage == null) throw new ArgumentNullException("handleMessage");
            _handleMessage = (msg, ctx, tok) => handleMessage(msg, ctx);
        }

        /// <summary>
        /// Initializes a new <see cref="DelegateMessageHandler"/> with the specified
        /// function delegate
        /// </summary>
        /// <param name="handleMessage">The function delegate that will handle the message</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="handleMessage"/> is
        /// <c>null</c></exception>
        public DelegateMessageHandler(Func<TContent, IMessageContext, CancellationToken, Task> handleMessage)
        {
            if (handleMessage == null) throw new ArgumentNullException("handleMessage");
            _handleMessage = handleMessage;
        }

        /// <summary>
        /// Initializes a new <see cref="DelegateMessageHandler"/> with the specified
        /// action delegate
        /// </summary>
        /// <param name="handleMessage">The action delegate that will handle the message</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="handleMessage"/> is
        /// <c>null</c></exception>
        public DelegateMessageHandler(Action<TContent, IMessageContext> handleMessage)
        {
            if (handleMessage == null) throw new ArgumentNullException("handleMessage");
            _handleMessage = (msg, ctx, tok) => Task.Run(() => handleMessage(msg, ctx), tok);
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
        /// <remarks>
        /// Invokes the function or action delegate specified in the constructor
        /// </remarks>
        public Task HandleMessage(object content, IMessageContext messageContext,
            CancellationToken cancellationToken)
        {
            return _handleMessage((TContent) content, messageContext, cancellationToken);
        }
    }
}