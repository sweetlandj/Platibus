#if NET452
// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Platibus.Http;
using Platibus.Http.Clients;
using Platibus.Serialization;
using Xunit;

namespace Platibus.IntegrationTests.OwinMiddleware
{
    [Trait("Category", "IntegrationTests")]
    [Collection(OwinMiddlewareCollection.Name)]
    public class OwinMiddlewarePubSubTests : PubSubTests
    {
        private readonly IHttpClientFactory _httpClientFactory = new BasicHttpClientFactory();

        public OwinMiddlewarePubSubTests(OwinMiddlewareFixture fixture)
            : base(fixture.Sender, fixture.Receiver)
        {
        }

        [Fact]
        public async Task TopicsCanBeQueriedOverHttp()
        {
            await AssertTopicsCanBeRetrievedByHttpClient();
        }

        [Fact]
        public async Task MessageJournalCanBeQueriedOverHttp()
        {
            GivenTestPublication();
            await WhenPublished();
            await AssertMessageRetrievedByMessageJournalClient();
        }

        protected async Task AssertMessageRetrievedByMessageJournalClient()
        {
            // From the platibus.owin0 configuration section of app.config
            var publisherBaseUri = new Uri("http://localhost:52182/platibus0/");
            using (var messageJournalClient = new HttpMessageJournalClient(publisherBaseUri))
            {
                var result = await messageJournalClient.Read(null, 100);
                Assert.NotNull(result);

                var messages = result.Entries.Select(e => e.Data);
                var messageContent = messages.Select(DeserializeMessageContent);
                Assert.Contains(Publication, messageContent);
            }
        }

        protected async Task AssertTopicsCanBeRetrievedByHttpClient()
        {
            // From the platibus.owin0 configuration section of app.config
            var publisherBaseUri = new Uri("http://localhost:52182/platibus0/");
            using (var httpClient = await _httpClientFactory.GetClient(publisherBaseUri, null))
            using (var responseMessage = await httpClient.GetAsync("topic"))
            {
                Assert.True(responseMessage.IsSuccessStatusCode, "HTTP Status " + responseMessage.StatusCode + " " + responseMessage.ReasonPhrase);
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var topicArray = JsonConvert.DeserializeObject<string[]>(responseContent);
                Assert.Contains((string)Topic, topicArray);
            }
        }

        private static object DeserializeMessageContent(Message message)
        {
            var messageNamingService = new DefaultMessageNamingService();
            var serializationService = new DefaultSerializationService();

            var messageType = messageNamingService.GetTypeForName(message.Headers.MessageName);
            var serializer = serializationService.GetSerializer(message.Headers.ContentType);
            return serializer.Deserialize(message.Content, messageType);
        }
    }
}
#endif