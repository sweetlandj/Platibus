using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Platibus.Http.Clients;
using Platibus.Journaling;
using Platibus.SampleWebApp.Models;
using Platibus.Security;

namespace Platibus.SampleWebApp.Controllers
{
    public class MessageJournalController : Controller
    {
        private static readonly Uri ApiBaseUri = new Uri("https://localhost:44313/platibus/");
        private static readonly HttpClientHandler ApiClientHandler;
        
        static MessageJournalController()
        {
            ApiClientHandler = new HttpClientHandler();
        }

        private async Task<MessageJournalIndexModel> InitIndexModel()
        {
            var model = new MessageJournalIndexModel
            {
                Count = 10,
                AllCategories = new string[]
                    {
                        MessageJournalCategory.Sent,
                        MessageJournalCategory.Received,
                        MessageJournalCategory.Published,
                    }
                    .Select(c => new SelectListItem { Text = c, Value = c })
                    .ToList()
            };

            using (var apiClient = NewApiClient())
            {
                var response = await apiClient.GetAsync("topic");
                response.EnsureSuccessStatusCode();
                var topicJson = await response.Content.ReadAsStringAsync();
                var topics = JsonConvert.DeserializeObject<string[]>(topicJson);
                model.AllTopics = topics
                    .Select(t => new SelectListItem { Text = t, Value = t })
                    .ToList();
            }

            return model;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            return View(await InitIndexModel());
        }

        [HttpPost]
        public async Task<ActionResult> Index(MessageJournalIndexModel model)
        {
            var updatedModel = await InitIndexModel();
            updatedModel.Start = model.Start;
            updatedModel.Count = model.Count;
            updatedModel.FilterCategories = model.FilterCategories;
            updatedModel.FilterTopics = model.FilterTopics;
            updatedModel.ReadAttempted = true;

            var accessToken = GetAccessToken();
            var credentials = new BearerCredentials(accessToken);
            using (var journalClient = new HttpMessageJournalClient(ApiBaseUri, credentials))
            {
                var filter = new MessageJournalFilter
                {
                    Topics = model.FilterTopics.Select(t => (TopicName)t).ToList(),
                    Categories = model.FilterCategories.Select(c => (MessageJournalCategory)c).ToList(),
                };
                var readResult = await journalClient.Read(model.Start, model.Count, filter);
                updatedModel.Result = readResult;
            }
            return View(updatedModel);
        }

        public async Task<ActionResult> Details(string position)
        {
            var accessToken = GetAccessToken();
            var credentials = new BearerCredentials(accessToken);
            using (var journalClient = new HttpMessageJournalClient(ApiBaseUri, credentials))
            {
                var readResult = await journalClient.Read(position, 1);
                return View(readResult.Entries.FirstOrDefault());
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