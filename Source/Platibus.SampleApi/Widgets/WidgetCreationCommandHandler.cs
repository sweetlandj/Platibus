using System.Threading;
using System.Threading.Tasks;
using Platibus.SampleMessages.Widgets;

namespace Platibus.SampleApi.Widgets
{
    public class WidgetCreationCommandHandler : IMessageHandler<WidgetCreationCommand>
    {
        private readonly IWidgetRepository _widgetRepository;

        public WidgetCreationCommandHandler(IWidgetRepository widgetRepository)
        {
            _widgetRepository = widgetRepository;
        }

        public async Task HandleMessage(WidgetCreationCommand content, IMessageContext messageContext, CancellationToken cancellationToken)
        {
            var widget = WidgetMap.ToEntity(content.Data);
            await _widgetRepository.Add(widget);
            var createdResource = WidgetMap.ToResource(widget);
            var principal = messageContext.Principal;
            var identity = principal == null ? null : principal.Identity;
            var requestor = identity == null ? null : identity.Name;
            await messageContext.Bus.Publish(new WidgetEvent("WidgetCreated", createdResource, requestor), "WidgetEvents", cancellationToken);
            messageContext.Acknowledge();
        }
    }
}