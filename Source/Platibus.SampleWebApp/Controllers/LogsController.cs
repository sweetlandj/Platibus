using System.Web.Mvc;

namespace Platibus.SampleWebApp.Controllers
{
    public class LogsController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View(DiagnosticEventLog.SingletonInstance.EmittedEvents);
        }
    }
}