using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Platibus.SampleMessages;
using Platibus.SampleMessages.Widgets;
using Platibus.Security;

namespace Platibus.SampleWebApp.Controllers
{
    public class WidgetsClient
    {
        private static readonly HttpClientHandler ClientHandler  = new HttpClientHandler();
        private readonly Uri _baseAddress = new Uri("https://localhost:44313/api/");
        private readonly string _accessToken;
        private readonly IBus _bus;

        public WidgetsClient(string accessToken, IBus bus)
        {
            _accessToken = accessToken;
            _bus = bus;
        }

        public WidgetsClient(string accessToken, Uri baseAddress)
        {
            _accessToken = accessToken;
            _baseAddress = baseAddress;
        }

        public async Task<IEnumerable<WidgetResource>> GetWidgets()
        {
            using (var client = NewClient())
            {
                var response = await client.GetAsync("widgets");
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseDocument = JsonConvert.DeserializeObject<ResponseDocument<IList<WidgetResource>>>(responseContent);
                return responseDocument.Data;
            }
        }

        public async Task<WidgetResource> GetWidget(string id)
        {
            using (var client = NewClient())
            {
                var response = await client.GetAsync("widgets/" + id);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseDocument = JsonConvert.DeserializeObject<ResponseDocument<WidgetResource>>(responseContent);
                return responseDocument.Data;
            }
        }

        public async Task<WidgetResource> CreateWidget(WidgetResource widget)
        {
            using (var client = NewClient())
            {
                var requestDocument = RequestDocument.Containing(widget);
                var response = await client.PostAsJsonAsync("widgets", requestDocument);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseDocument = JsonConvert.DeserializeObject<ResponseDocument<WidgetResource>>(responseContent);
                return responseDocument.Data;
            }
        }

        public async Task CreateWidgetAsync(WidgetResource widget)
        {
            var command = new WidgetCreationCommand(widget);
            var sendOptions = new SendOptions {Credentials = new BearerCredentials(_accessToken)};
            await _bus.Send(command, sendOptions);
        }

        public async Task<WidgetResource> UpdateWidget(string id, WidgetResource updates)
        {
            using (var client = NewClient())
            {
                var requestDocument = RequestDocument.Containing(updates);
                var requestContent = JsonConvert.SerializeObject(requestDocument);
                var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), "widgets/" + id)
                {
                    Content = new StringContent(requestContent, Encoding.UTF8, "application/json")
                };

                var response = await client.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseDocument = JsonConvert.DeserializeObject<ResponseDocument<WidgetResource>>(responseContent);
                return responseDocument.Data;
            }
        }

        public async Task DeleteWidget(string id)
        {
            using (var client = NewClient())
            {
                var response = await client.DeleteAsync("widgets/" + id);
                response.EnsureSuccessStatusCode();
            }
        }

        private HttpClient NewClient()
        {
            var client = new HttpClient(ClientHandler, false);
            if (_baseAddress != null)
            {
                client.BaseAddress = _baseAddress;
            }

            if (!string.IsNullOrWhiteSpace(_accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }

            return client;
        }
    }
}