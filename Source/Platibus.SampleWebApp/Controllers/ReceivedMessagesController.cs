using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Common.Logging;
using Platibus.SampleWebApp.Models;

namespace Platibus.SampleWebApp.Controllers
{
    [Authorize(Roles = "test")]
    public class ReceivedMessagesController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(SampleWebAppLoggingCategories.SampleWebApp);

        private readonly ReceivedMessageRepository _repository;

        public ReceivedMessagesController(ReceivedMessageRepository repository)
        {
            _repository = repository;
        }

        // GET: ReceivedMessages
        public async Task<ActionResult> Index()
        {
            Log.DebugFormat("[Process {0}, Thread {1}, AppDomain {2}]",
                Process.GetCurrentProcess().Id,
                Thread.CurrentThread.ManagedThreadId,
                AppDomain.CurrentDomain.Id);

            return View("Index", new ReceivedMessages(
                (await _repository.GetMessages())
                    .OrderByDescending(msg => msg.Received)));
        }

        public async Task<ActionResult> Clear()
        {
            await _repository.RemoveAll();
            return await Index();
        }

        public async Task<ActionResult> Remove(string messageId)
        {
            await _repository.Remove(messageId);
            return await Index();
        }

        public async Task<ActionResult> ReceivedMessage(string messageId)
        {
            var receivedMessage = await _repository.Get(messageId);
            return View(receivedMessage);
        }
    }
}