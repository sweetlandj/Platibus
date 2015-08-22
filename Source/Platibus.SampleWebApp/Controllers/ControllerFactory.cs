using System.Web.Mvc;
using System.Web.Routing;

namespace Platibus.SampleWebApp.Controllers
{
    public class ControllerFactory : DefaultControllerFactory
    {
        private readonly ReceivedMessageRepository _receivedMessageRepository;

        public ControllerFactory()
        {
            _receivedMessageRepository = new ReceivedMessageRepository();
            _receivedMessageRepository.Init();
        }

        public override IController CreateController(RequestContext requestContext, string controllerName)
        {
            if ("ReceivedMessages".Equals(controllerName))
            {
                return new ReceivedMessagesController(_receivedMessageRepository);
            }
            return base.CreateController(requestContext, controllerName);
        }
    }
}