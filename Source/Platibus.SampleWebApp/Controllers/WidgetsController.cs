using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Platibus.Owin;
using Platibus.SampleMessages.Widgets;
using Platibus.Security;

namespace Platibus.SampleWebApp.Controllers
{
    public class WidgetsController : Controller
    {
        private WidgetsClient WidgetClient
        {
            get
            {
                var bus = HttpContext.GetOwinContext().GetBus();
                return new WidgetsClient(GetAccessToken(), bus);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var model = await WidgetClient.GetWidgets();
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Details(string id)
        {
            var model = await WidgetClient.GetWidget(id);
            return View(model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var model = new WidgetResource
            {
                Attributes = new WidgetAttributes()
            };
            return View(model);
        }

        [HttpPost]
        [SubmitAction("action", "Create")]
        public async Task<ActionResult> Create(WidgetResource model)
        {
            try
            {
                await WidgetClient.CreateWidget(model);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error creating widget: " + ex.Message;
                return View("Create", model);
            }
        }

        [HttpPost]
        [SubmitAction("action", "Create (Async)")]
        public async Task<ActionResult> CreateAsync(WidgetResource model)
        {
            try
            {
                await WidgetClient.CreateWidgetAsync(model);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error creating widget: " + ex.Message;
                return View("Create", model);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Edit(string id)
        {
            var model = await WidgetClient.GetWidget(id);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(string id, WidgetResource model)
        {
            try
            {
                await WidgetClient.UpdateWidget(id, model);
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                TempData["Error"] = "Error updating widget: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Delete(string id)
        {
            var model = await WidgetClient.GetWidget(id);
            
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(string id, WidgetResource model)
        {
            try
            {
                await WidgetClient.DeleteWidget(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting widget: " + ex.Message;
                return View(model);
            }
        }
        
        private string GetAccessToken()
        {
            // The name of the claim containing the access token may vary depending on the
            // callback registered with the SecurityTokenValidated notification in the
            // OpenIdConnectAuthentication middleware.  See the OnSecurityTokenValidated
            // callback method the Startup class.
            return HttpContext.User.GetClaimValue("access_token");
        }
    }
}
