using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Pluribus.IIS;
using Pluribus.SampleWebApp.Models;

namespace Pluribus.SampleWebApp.Controllers
{
    public class SendTestMessageController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var bus = await BusManager.GetInstance();
            return View(new SendTestMessage
            {
                MessageId = MessageId.Generate(),
                Destination = bus.BaseUri.ToString(),
                ContentType = "application/json"
            });
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

                var bus = await BusManager.GetInstance();
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