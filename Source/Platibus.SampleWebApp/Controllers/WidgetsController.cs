using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Platibus.SampleMessages;
using Platibus.SampleMessages.Widgets;
using Platibus.Security;

namespace Platibus.SampleWebApp.Controllers
{
    public class WidgetsController : Controller
    {
        private static readonly Uri ApiBaseUri = new Uri("https://localhost:44313/api/");
        private static readonly HttpClientHandler ApiClientHandler;

        static WidgetsController()
        {
            ApiClientHandler = new HttpClientHandler();
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            IList<WidgetResource> model;
            using (var apiClient = NewApiClient())
            {
                var response = await apiClient.GetAsync("widgets");
                response.EnsureSuccessStatusCode();
                var widgetsJson = await response.Content.ReadAsStringAsync();
                var responseDocument = JsonConvert.DeserializeObject<ResponseDocument<IList<WidgetResource>>>(widgetsJson);
                model = responseDocument.Data;
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            return View();
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
        public async Task<ActionResult> Create(WidgetResource model)
        {
            try
            {
                using (var apiClient = NewApiClient())
                {
                    var requestDocument = RequestDocument.Containing(model);
                    var response = await apiClient.PostAsJsonAsync("widgets", requestDocument);
                    response.EnsureSuccessStatusCode();
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        private HttpClient NewApiClient()
        {
            var accessToken = GetAccessToken();
            return new HttpClient(ApiClientHandler, false)
            {
                BaseAddress = ApiBaseUri,
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", accessToken)
                }
            };
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
