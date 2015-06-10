using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Platibus.IIS;
using Platibus.SampleWebApp.Models;

namespace Platibus.SampleWebApp.Controllers
{
    public class SendTestMessageController : Controller
    {
        private readonly IBusManager _busManager;

        public SendTestMessageController()
        {
            _busManager = BusManager.GetInstance();
        }

        public ActionResult Index()
        {
            return View(new SendTestMessage
            {
                MessageId = MessageId.Generate(),
                Destination = "http://localhost:52180/platibus",
                ContentType = "application/json"
            });
        }

        public async Task<ActionResult> SendTestMessage(SendTestMessage sendTestMessage)
        {
            try
            {
                var sendOptions = new SendOptions
                {
                    ContentType = sendTestMessage.ContentType,
                    Importance = sendTestMessage.Importance
                };

                var message = new TestMessage
                {
                    Text = sendTestMessage.MessageText
                };

                var bus = await _busManager.GetBus();
                var sentMessage = await bus.Send(message, sendOptions);
                return View("Index", new SendTestMessage
                {
                    MessageSent = true,
                    SentMessageId = sentMessage.MessageId
                });
            }
            catch (Exception ex)
            {
                sendTestMessage.ErrorsOccurred = true;
                sendTestMessage.ErrorMessage = ex.ToString();
            }
            return View("Index", sendTestMessage);
        }
    }
}