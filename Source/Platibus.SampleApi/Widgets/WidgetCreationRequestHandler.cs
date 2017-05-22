using System.Threading;
using System.Threading.Tasks;
using Platibus.SampleMessages.Widgets;

namespace Platibus.SampleApi.Widgets
{
    public class WidgetCreationRequestHandler : IMessageHandler<WidgetCreationRequest>
    {
        public Task HandleMessage(WidgetCreationRequest content, IMessageContext messageContext, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}