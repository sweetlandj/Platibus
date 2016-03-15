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
            if (genericMessageHandler == null) throw new ArgumentNullException("genericMessageHandler");
            _genericMessageHandler = genericMessageHandler;
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
