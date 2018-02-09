using Platibus.Diagnostics;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Config
{
    internal class HandlerActivation : IMessageHandler
    {
        private readonly IDiagnosticService _diagnosticService;
        private readonly Type _handlerType;
        private readonly Func<object> _handlerFactory;
        private readonly MethodInfo _method;

        public HandlerActivation(IDiagnosticService diagnosticService, Type handlerType, Func<object> handlerFactory, MethodInfo method)
        {
            _diagnosticService = diagnosticService;
            _handlerType = handlerType;
            _handlerFactory = handlerFactory;
            _method = method;
        }

        public async Task HandleMessage(object content, IMessageContext messageContext, CancellationToken cancellationToken)
        {
            try
            {
                var handler = _handlerFactory();
                if (handler == null && !_method.IsStatic)
                {
                    throw new NullReferenceException("Handler factory returned null handler instance");
                }
                await (Task)_method.Invoke(handler, new[] { content, messageContext, cancellationToken });
            }
            catch (Exception e)
            {
                _diagnosticService.Emit(new DiagnosticEventBuilder(null, DiagnosticEventType.HandlerActivationError)
                {
                    Detail = $"Error activating instance of message handler {_handlerType}: {e.Message}",
                    Exception = e
                }.Build());
                throw;
            }
        }
    }
}
