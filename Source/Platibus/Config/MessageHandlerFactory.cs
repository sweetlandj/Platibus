using System;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Config
{
    /// <summary>
    /// Adapter that uses a factory method to get or create a handler before each message is 
    /// handled
    /// </summary>
    public class MessageHandlerFactory : IMessageHandler
    {
        private readonly Func<IMessageHandler> _factory;

        /// <summary>
        /// Initializes a new <see cref="MessageHandlerFactory"/> from the specified 
        /// <paramref name="factory"/> method
        /// </summary>
        /// <param name="factory">A method that produces the <see cref="IMessageHandler"/> instance
        /// that will process messages (e.g. container lookup)</param>
        /// <exception cref="ArgumentNullException"></exception>
        public MessageHandlerFactory(Func<IMessageHandler> factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            _factory = factory;
        }

        /// <inheritdoc />
        public Task HandleMessage(object content, IMessageContext messageContext, CancellationToken cancellationToken)
        {
            var inner = _factory();
            return inner.HandleMessage(content, messageContext, cancellationToken);
        }

        /// <summary>
        /// Static factory method that initializes a new <see cref="MessageHandlerFactory"/> from 
        /// the specified <paramref name="factory"/> method
        /// </summary>
        /// <param name="factory">A method that produces the <see cref="IMessageHandler"/> instance
        /// that will process messages (e.g. container lookup)</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static MessageHandlerFactory For(Func<IMessageHandler> factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return new MessageHandlerFactory(factory);
        }
    }

    /// <summary>
    /// Adapter that uses a factory method to get or create a handler before each message is 
    /// handled
    /// </summary>
    /// <typeparam name="TContent">The type of content expected by the handler</typeparam>
    public class MessageHandlerFactory<TContent> : IMessageHandler<TContent>, IMessageHandler
    {
        private readonly Func<IMessageHandler<TContent>> _factory;

        /// <summary>
        /// Initializes a new <see cref="MessageHandlerFactory"/> from the specified 
        /// <paramref name="factory"/> method
        /// </summary>
        /// <param name="factory">A method that produces the <see cref="IMessageHandler"/> instance
        /// that will process messages (e.g. container lookup)</param>
        /// <exception cref="ArgumentNullException"></exception>
        public MessageHandlerFactory(Func<IMessageHandler<TContent>> factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            _factory = factory;
        }

        /// <inheritdoc />
        public Task HandleMessage(TContent content, IMessageContext messageContext, CancellationToken cancellationToken)
        {
            var inner = _factory();
            return inner.HandleMessage(content, messageContext, cancellationToken);
        }

        /// <inheritdoc />
        public Task HandleMessage(object content, IMessageContext messageContext, CancellationToken cancellationToken)
        {
            var inner = new GenericMessageHandlerAdapter<TContent>(_factory());
            return inner.HandleMessage(content, messageContext, cancellationToken);
        }

        /// <summary>
        /// Static factory method that initializes a new <see cref="MessageHandlerFactory"/> from 
        /// the specified <paramref name="factory"/> method
        /// </summary>
        /// <param name="factory">A method that produces the <see cref="IMessageHandler"/> instance
        /// that will process messages (e.g. container lookup)</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static MessageHandlerFactory<T> For<T>(Func<IMessageHandler<T>> factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return new MessageHandlerFactory<T>(factory);
        }
    }
}
