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
    public class ViewReceivedMessagesController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(SampleWebAppLoggingCategories.SampleWebApp);

        private readonly ReceivedMessageRepository _repository;

        public ViewReceivedMessagesController(ReceivedMessageRepository repository)
        {
            _repository = repository;
        }

        // GET: ViewReceivedMessages
        public async Task<ActionResult> Index()
        {
            Log.DebugFormat("[Process {0}, Thread {1}, AppDomain {2}]",
                Process.GetCurrentProcess().Id,
                Thread.CurrentThread.ManagedThreadId,
                AppDomain.CurrentDomain.Id);

            return View("Index", new ViewReceivedMessages
            {
                ReceivedMessages = (await _repository.GetMessages())
                    .OrderByDescending(msg => msg.Received)
                    .ToList()
            });
        }

        public async Task<ActionResult> ClearReceivedMessages()
        {
            await _repository.RemoveAll();
            return await Index();
        }

        public async Task<ActionResult> RemoveReceivedMessage(string messageId)
        {
            await _repository.Remove(messageId);
            return await Index();
        }

        public async Task<ActionResult> ViewReceivedMessageDetail(string messageId)
        {
            var receivedMessage = await _repository.Get(messageId);
            return View("ReceivedMessageDetail", receivedMessage);
        }
    }
}