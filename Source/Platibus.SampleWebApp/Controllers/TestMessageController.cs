using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Platibus.IIS;
using Platibus.Security;

namespace Platibus.SampleWebApp.Controllers
{
    [Authorize(Roles = "test")]
    public class TestMessageController : Controller
    {
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
                // The name of the claim containing the access token may vary depending on the
                // callback registered with the SecurityTokenValidated notification in the
                // OpenIdConnectAuthentication middleware.  See the AugmentClaims 
                var accessToken = HttpContext.User.GetClaimValue("access_token");
                var sendOptions = new SendOptions
                {
                    ContentType = testMessage.ContentType,
                    Importance = testMessage.Importance,
                    Credentials = new BearerCredentials(accessToken)
                };

                var message = new TestMessage
                {
                    Text = testMessage.MessageText
                };

                var bus = HttpContext.GetBus();
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