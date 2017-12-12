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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using Platibus.Http;
using Platibus.Http.Controllers;
using Platibus.Http.Models;
using Platibus.Journaling;
using Platibus.Security;
using Xunit;

namespace Platibus.UnitTests.Http
{
    [Trait("Category", "UnitTests")]
    public class JournalControllerTests : IDisposable
    {
        protected IMessageJournal MessageJournal;
        protected IAuthorizationService AuthorizationService;
        protected IHttpResourceRequest Request;

        protected MemoryStream ResponseContent;
        protected Mock<IHttpResourceResponse> Response;

        public JournalControllerTests()
        {
            MessageJournal = new MessageJournalStub();
            ResponseContent = new MemoryStream();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ResponseContent.Dispose();
            }
        }

        [Fact]
        public async Task NotImplementedWhenJournalingNotEnabled()
        {
            GivenMessageJournalingDisabled();
            GivenRequest("GET", new NameValueCollection
            {
                {"count", "10"}
            });
            await WhenProcessingGetRequest();
        }

        [Fact]
        public async Task AuthorizationServiceCanPreventUnauthorizedRequests()
        {
            GivenNotAuthorizedToQueryJournal();
            GivenRequest("GET", new NameValueCollection
            {
                {"count", "10"}
            });
            await WhenProcessingGetRequest();
            AssertUnauthorized();
        }

        [Fact]
        public async Task AuthorizationServiceCanExplicitlyAuthorizeRequests()
        {
            GivenExplicitAuthorizationToQueryJournal();
            GivenRequest("GET", new NameValueCollection
            {
                {"count", "10"}
            });
            await WhenProcessingGetRequest();
            AssertSuccess();
        }

        [Fact]
        public async Task ParametersAreCorrectlyMapped()
        {
            var mockMessageJournal = GiveMockMessageJournal();
            GivenRequest("GET", new NameValueCollection
            {
                {"start", "20"},
                {"count", "10"},
                {"topic", "FooEvents, BarEvents"},
                {"category", "Received, Published"},
                {"from", "2017-08-09T15:31:11.012"},
                {"to", "2017-08-10"},
                {"origination", "http://localhost:8089/platibus"},
                {"destination", "http://localhost:8090/platibus"},
                {"messageName", "event"},
                {"relatedTo", "{637D88F6-48AC-4521-BD45-EA0965022AEC}"}
            });

            await WhenProcessingGetRequest();
            AssertSuccess();

            var expectedStart = new MessageJournalStub.Position(20);
            mockMessageJournal.Verify(x => x.Read(expectedStart, 10,
                It.Is<MessageJournalFilter>(mf => IsExpectedFilter(mf)),
                It.IsAny<CancellationToken>()));
        }

        private static bool IsExpectedFilter(MessageJournalFilter mf)
        {
            Assert.Equal(2, mf.Topics.Count);
            Assert.Equal(2, mf.Categories.Count);
            Assert.Contains((TopicName) "FooEvents", mf.Topics);
            Assert.Contains((TopicName) "BarEvents", mf.Topics);
            Assert.Contains((MessageJournalCategory) "Received", mf.Categories);
            Assert.Contains((MessageJournalCategory) "Received", mf.Categories);
            Assert.Equal(new DateTime(2017, 8, 9, 15, 31, 11, DateTimeKind.Utc).AddMilliseconds(12), mf.From);
            Assert.Equal(new DateTime(2017, 8, 10, 0, 0, 0, DateTimeKind.Utc), mf.To);
            Assert.Equal(new Uri("http://localhost:8089/platibus"), mf.Origination);
            Assert.Equal(new Uri("http://localhost:8090/platibus"), mf.Destination);
            Assert.Equal("event", mf.MessageName);
            Assert.Equal(new Guid("{637D88F6-48AC-4521-BD45-EA0965022AEC}"), mf.RelatedTo);

            return true;
        }

        [Fact]
        public async Task MessageJournalReadResultIsCorrectlyMappedInResponse()
        {
            var messageHeaders = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                Published = DateTime.UtcNow,
                Origination = new Uri("urn:localhost/platibus0"),
                MessageName = "FooStarted",
                ContentType = "text/plain",
                Topic = "FooEvents"
            };
            var message = new Message(messageHeaders, "FooStarted:1");
            await MessageJournal.Append(message, MessageJournalCategory.Published);
            GivenRequest("GET", new NameValueCollection
            {
                {"count", "10"}
            });
            await WhenProcessingGetRequest();
            AssertSuccess();

            var responseModel = AssertResponseContentModel();
            Assert.Equal("0", responseModel.Start);
            Assert.Equal("1", responseModel.Next);
            Assert.True(responseModel.EndOfJournal);
            Assert.Equal(1, responseModel.Entries.Count);

            var entryModel = responseModel.Entries[0];
            Assert.NotNull(entryModel);
            Assert.Equal("0", entryModel.Position);
            Assert.NotEqual(default(DateTime), entryModel.Timestamp);
            Assert.Equal(DateTimeKind.Utc, entryModel.Timestamp.Kind);
            Assert.Equal("Published", entryModel.Category);
            Assert.NotNull(entryModel.Data);

            var actualHeaders = new MessageHeaders(entryModel.Data.Headers);
            Assert.Equal(messageHeaders, actualHeaders, new MessageHeadersEqualityComparer());
            Assert.Equal(message.Content, entryModel.Data.Content);
        }

        [Theory]
        [InlineData("POST")]
        [InlineData("PATCH")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        public async Task OnlyGetMethodRequestsAreAllowed(string method)
        {
            GivenRequest(method);
            await WhenProcessingGetRequest();
            AssertMethodNotAllowed();
        }
        
        [Fact]
        public async Task BadRequestWhenMissingCountParameter()
        {
            GivenRequest("GET", new NameValueCollection());
            await WhenProcessingGetRequest();
            AssertBadRequest();
            AssertParameterErrorInResponse("count");
        }

        [Theory]
        [InlineData("ten")]
        [InlineData("A")]
        [InlineData("0xA")]
        [InlineData("10.5")]
        [InlineData("1,000")]
        [InlineData("2147483648")]
        public async Task BadRequestWhenStartParameterInvalid(string start)
        {
            GivenRequest("GET", new NameValueCollection
            {
                {"start", start},
                {"count", "10"}
            });
            await WhenProcessingGetRequest();
            AssertBadRequest();
            AssertParameterErrorInResponse("start");
        }

        [Theory]
        [InlineData("")]
        [InlineData("ten")]
        [InlineData("A")]
        [InlineData("0xA")]
        [InlineData("0")]
        [InlineData("-1")]
        [InlineData("10.5")]
        [InlineData("1,000")]
        [InlineData("2147483648")]
        public async Task BadRequestWhenCountParameterInvalid(string count)
        {
            GivenRequest("GET", new NameValueCollection
            {
                {"count", count}
            });
            await WhenProcessingGetRequest();
            AssertBadRequest();
            AssertParameterErrorInResponse("count");
        }

        protected Mock<MessageJournalStub> GiveMockMessageJournal()
        {
            var mockMessageJournal = new Mock<MessageJournalStub>
            {
                CallBase = true
            };
            MessageJournal = mockMessageJournal.Object;
            return mockMessageJournal;
        }

        protected void GivenMessageJournalingDisabled()
        {
            MessageJournal = null;
        }

        protected void GivenNotAuthorizedToQueryJournal()
        {
            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(x => x.IsAuthorizedToQueryJournal(It.IsAny<IPrincipal>())).ReturnsAsync(false);
            AuthorizationService = mockAuthorizationService.Object;
        }

        protected void GivenExplicitAuthorizationToQueryJournal()
        {
            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(x => x.IsAuthorizedToQueryJournal(It.IsAny<IPrincipal>())).ReturnsAsync(true);
            AuthorizationService = mockAuthorizationService.Object;
        }

        protected void GivenRequest(string method, NameValueCollection parameters = null)
        {
            var mockRequest = new Mock<IHttpResourceRequest>();
            mockRequest.Setup(x => x.HttpMethod).Returns(method);
            mockRequest.Setup(x => x.QueryString).Returns(parameters ?? new NameValueCollection());
            Request = mockRequest.Object;
        }

        protected async Task WhenProcessingGetRequest()
        {
            Response = new Mock<IHttpResourceResponse>();
            Response.Setup(x => x.ContentEncoding).Returns(Encoding.UTF8);
            Response.Setup(x => x.OutputStream).Returns(ResponseContent);
            var controller = new JournalController(MessageJournal, AuthorizationService);
            await controller.Process(Request, Response.Object, Enumerable.Empty<string>());
        }

        protected void AssertBadRequest()
        {
            Response.VerifySet(r => r.StatusCode = (int)HttpStatusCode.BadRequest);
        }

        protected void AssertMethodNotAllowed()
        {
            Response.VerifySet(r => r.StatusCode = (int)HttpStatusCode.MethodNotAllowed);
            Response.Verify(r => r.AddHeader("Allow", "GET"));
        }

        protected void AssertUnauthorized()
        {
            Response.VerifySet(r => r.StatusCode = (int)HttpStatusCode.Unauthorized);
        }

        protected void AssertNotImplemented()
        {
            Response.VerifySet(r => r.StatusCode = (int)HttpStatusCode.NotImplemented);
        }

        protected void AssertSuccess()
        {
            Response.VerifySet(r => r.StatusCode = (int)HttpStatusCode.OK);
        }

        protected void AssertParameterErrorInResponse(string parameter)
        {
            var responseModel = AssertResponseContentModel();
            Assert.NotNull(responseModel);
            Assert.NotEmpty(responseModel.Errors);
            Assert.Contains(responseModel.Errors, e => Equals(parameter, e.Parameter));
        }

        protected JournalGetResponseModel AssertResponseContentModel()
        {
            var responseBytes = ResponseContent.GetBuffer();
            var json = Encoding.UTF8.GetString(responseBytes);
            var responseModel = JsonConvert.DeserializeObject<JournalGetResponseModel>(json);
            Assert.NotNull(responseModel);
            return responseModel;
        }
    }
}