using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Platibus.IIS;

namespace Platibus.SampleWebApp.Controllers
{
    public class TestMessageController : Controller
    {
        private readonly IBusManager _busManager;

        public TestMessageController()
        {
            _busManager = BusManager.GetInstance();
        }

        public ActionResult Index()
        {
            return View(new Models.TestMessage
            {
                MessageId = MessageId.Generate(),
                Destination = "http://localhost:52180/platibus",
                ContentType = "application/json"
            });
        }

        public async Task<ActionResult> SendTestMessage(Models.TestMessage testMessage)
        {
            try
            {
                var sendOptions = new SendOptions
                {
                    ContentType = testMessage.ContentType,
                    Importance = testMessage.Importance
                };

                var message = new TestMessage
                {
                    Text = testMessage.MessageText
                };

                var bus = await _busManager.GetBus();
                var sentMessage = await bus.Send(message, sendOptions);
                return View("Index", new Models.TestMessage
                {
                    MessageSent = true,
                    SentMessageId = sentMessage.MessageId
                });
            }
            catch (Exception ex)
            {
                testMessage.ErrorsOccurred = true;
                testMessage.ErrorMessage = ex.ToString();
            }
            return View("Index", testMessage);
        }
    }
}