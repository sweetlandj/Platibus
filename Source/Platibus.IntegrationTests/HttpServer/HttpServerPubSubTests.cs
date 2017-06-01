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
using Platibus.Http.Clients;
using Platibus.Serialization;
using Xunit;

namespace Platibus.IntegrationTests.HttpServer
{
    [Trait("Category", "IntegrationTests")]
    [Collection(HttpServerCollection.Name)]
    public class HttpServerPubSubTests : PubSubTests
    {
        public HttpServerPubSubTests(HttpServerFixture fixture)
            : base(fixture.Sender, fixture.Receiver)
        {
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
            // From the platibus.http0 configuration section of app.config
            var publisherBaseUri = new Uri("http://localhost:52180/platibus0/");
            var messageJournalClient = new HttpMessageJournalClient(publisherBaseUri);
            var result = await messageJournalClient.Read(null, 100);
            Assert.NotNull(result);

            var messages = result.Entries.Select(e => e.Data);
            var messageContent = messages.Select(DeserializeMessageContent);
            Assert.Contains(Publication, messageContent);
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