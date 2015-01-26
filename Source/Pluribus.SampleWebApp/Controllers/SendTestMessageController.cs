using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Pluribus.IIS;
using Pluribus.SampleWebApp.Models;

namespace Pluribus.SampleWebApp.Controllers
{
    public class SendTestMessageController : Controller
    {
        private readonly Bus _bus;

        public SendTestMessageController()
        {
            _bus = BusManager.GetInstance().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        // GET: SendTestMessage
        public ActionResult Index()
        {
            return View(NewModel());
        }

        public async Task<ActionResult> SendTestMessage(SendTestMessage sendTestMessage)
        {
            try
            {
                var sendOptions = new SendOptions
                {
                    UseDurableTransport = sendTestMessage.UseDurableTransport,
                    ContentType = sendTestMessage.ContentType
                };

                var message = new TestMessage
                {
                    Text = sendTestMessage.MessageText
                };

                var sentMessage = await _bus.Send(message, sendOptions).ConfigureAwait(false);
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

        private SendTestMessage NewModel()
        {
            return new SendTestMessage
            {
                MessageId = MessageId.Generate(),
                Destination = _bus.BaseUri.ToString(),
                ContentType = "application/json"
            };
        }
    }
}