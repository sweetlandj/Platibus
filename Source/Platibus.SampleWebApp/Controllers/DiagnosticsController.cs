using System.Web;
using System.Web.Mvc;
using Platibus.IIS;
using Platibus.Owin;
using Platibus.SampleWebApp.Models;

namespace Platibus.SampleWebApp.Controllers
{
    public class DiagnosticsController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View(new DiagnosticsIndexModel
            {
                Requests = 100,
                MinTime = 50,
                MaxTime = 250,
                AcknowledgementRate = 0.80,
                ReplyRate = 0.75,
                ErrorRate = 0.10
            });
        }

        [HttpPost]
        public ActionResult Index(DiagnosticsIndexModel model)
        {
            // This is only necessary since this sample app can toggle between 
            // multiple different configurations
            var bus = SampleWebAppSetting.OwinMiddleware.IsEnabled()
                ? HttpContext.GetOwinContext().GetBus()
                : HttpContext.GetBus();

            var simulator = new RequestSimulator(bus, model.Requests, model.MinTime, model.MaxTime,
                model.AcknowledgementRate, model.ReplyRate, model.ErrorRate);

            simulator.Start();

            return RedirectToAction("Monitoring");
        }

        public ActionResult Monitoring()
        {
            return View(new DiagnosticsMonitoringModel
            {
                BaseUri = "https://localhost:44313/platibus/"
            });
        }
    }
}