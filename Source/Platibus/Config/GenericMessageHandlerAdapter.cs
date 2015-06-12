using System;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Config
{
    class GenericMessageHandlerAdapter
    {
        public static GenericMessageHandlerAdapter<TContent> For<TContent>(IMessageHandler<TContent> genericMessageHandler)
        {
            return new GenericMessageHandlerAdapter<TContent>(genericMessageHandler);
        }
    }

    class GenericMessageHandlerAdapter<TContent> : IMessageHandler
    {
        private readonly IMessageHandler<TContent> _genericMessageHandler;

        public GenericMessageHandlerAdapter(IMessageHandler<TContent> genericMessageHandler)
        {
            if (genericMessageHandler == null) throw new ArgumentNullException("genericMessageHandler");
            _genericMessageHandler = genericMessageHandler;
        }

        public Task HandleMessage(object message, IMessageContext messageContext, CancellationToken cancellationToken)
        {
            return _genericMessageHandler.HandleMessage((TContent) message, messageContext, cancellationToken);
        }        
    }
}
