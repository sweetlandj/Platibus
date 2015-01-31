using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Common.Logging;
using Pluribus.SampleWebApp.Models;

namespace Pluribus.SampleWebApp.Controllers
{
    public class ViewReceivedMessagesController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(SampleWebAppLoggingCategories.SampleWebApp);

        private readonly ReceivedMessageRepository _repository;

        public ViewReceivedMessagesController()
        {
            _repository = ReceivedMessageRepository.SingletonInstance;
        }

        // GET: ViewReceivedMessages
        public ActionResult Index()
        {
            Log.DebugFormat("[Process {0}, Thread {1}, AppDomain {2}]",
                Process.GetCurrentProcess().Id,
                Thread.CurrentThread.ManagedThreadId,
                AppDomain.CurrentDomain.Id);

            return View(new ViewReceivedMessages
            {
                ReceivedMessages = _repository
                    .GetMessages()
                    .OrderByDescending(msg => msg.Received)
                    .ToList()
            });
        }

        public ActionResult ClearReceivedMessages()
        {
            _repository.RemoveAll();
            return Index();
        }

        public ActionResult RemoveReceivedMessage(string messageId)
        {
            _repository.Remove(messageId);
            return Index();
        }

        public ActionResult ViewReceivedMessageDetail(string messageId)
        {
            var receivedMessage = _repository.Get(messageId);
            return View("ReceivedMessageDetail", receivedMessage);
        }
    }
}