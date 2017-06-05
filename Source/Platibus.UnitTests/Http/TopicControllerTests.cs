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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Moq;
using Xunit;
using Platibus.Http;
using Platibus.Http.Controllers;
using Platibus.Serialization;

namespace Platibus.UnitTests.Http
{
    [Trait("Category", "UnitTests")]
    public class TopicControllerTests
    {
        [Fact]
        public async Task GetRequestToTopicResourceReturnsListOfTopics()
        {
            var mockSubscriptionTrackingService = new Mock<ISubscriptionTrackingService>();
            var topics = new TopicName[] { "topic-1", "topic-2", "topic-3" };
            var controller = new TopicController(mockSubscriptionTrackingService.Object, topics);

            var mockRequest = new Mock<IHttpResourceRequest>();
            var requestUri = new UriBuilder
            {
                Scheme = "http",
                Host = "localhost",
                Path = "/platibus/topic/"
            }.Uri;

            mockRequest.Setup(r => r.HttpMethod).Returns("GET");
            mockRequest.Setup(r => r.Url).Returns(requestUri);

            var mockResponse = new Mock<IHttpResourceResponse>();
            var responseOutputStream = new MemoryStream();
            var responseEncoding = Encoding.UTF8;
            mockResponse.Setup(r => r.ContentEncoding).Returns(responseEncoding);
            mockResponse.Setup(r => r.OutputStream).Returns(responseOutputStream);

            await controller.Process(mockRequest.Object, mockResponse.Object, Enumerable.Empty<string>());

            mockResponse.VerifySet(r => r.StatusCode = 200);
            var responseContent = responseEncoding.GetString(responseOutputStream.GetBuffer());
            var topicsFromResponse = new NewtonsoftJsonSerializer().Deserialize<string[]>(responseContent);

            Assert.Equal(3, topicsFromResponse.Length);
            Assert.Contains("topic-1", topicsFromResponse);
            Assert.Contains("topic-2", topicsFromResponse);
            Assert.Contains("topic-3", topicsFromResponse);
        }

        [Fact]
        public async Task BadRequestWhenPostingToUnknownTopic()
        {
            var mockSubscriptionTrackingService = new Mock<ISubscriptionTrackingService>();
            var topics = new TopicName[] { "topic-1", "topic-2", "topic-3" };
            var controller = new TopicController(mockSubscriptionTrackingService.Object, topics);

            var mockRequest = new Mock<IHttpResourceRequest>();
            var requestUri = new UriBuilder
            {
                Scheme = "http",
                Host = "localhost",
                Path = "/platibus/topic/"
            }.Uri;
            mockRequest.Setup(r => r.HttpMethod).Returns("POST");
            mockRequest.Setup(r => r.Url).Returns(requestUri);

            var mockResponse = new Mock<IHttpResourceResponse>();
            var responseOutputStream = new MemoryStream();
            var responseEncoding = Encoding.UTF8;
            mockResponse.Setup(r => r.ContentEncoding).Returns(responseEncoding);
            mockResponse.Setup(r => r.OutputStream).Returns(responseOutputStream);

            await controller.Process(mockRequest.Object, mockResponse.Object, Enumerable.Empty<string>());

            mockResponse.VerifySet(r => r.StatusCode = 400);
        }

        [Fact]
        public async Task ValidPostRequestToTopicSubscriberResourceCreatesSubscription()
        {
            var mockSubscriptionTrackingService = new Mock<ISubscriptionTrackingService>();
            mockSubscriptionTrackingService.Setup(sts =>
                sts.AddSubscription(It.IsAny<TopicName>(), It.IsAny<Uri>(), It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            var topics = new TopicName[] { "topic-1", "topic-2", "topic-3" };
            var controller = new TopicController(mockSubscriptionTrackingService.Object, topics);

            var mockRequest = new Mock<IHttpResourceRequest>();
            var encodedSubscriberUri = HttpUtility.UrlEncode("http://example.com/platibus");
            var requestUri = new UriBuilder
            {
                Scheme = "http",
                Host = "localhost",
                Path = "/platibus/topic/topic-1/subscriber",
                Query = string.Format("uri={0}&ttl=3600", encodedSubscriberUri)
            }.Uri;

            mockRequest.Setup(r => r.HttpMethod).Returns("POST");
            mockRequest.Setup(r => r.Url).Returns(requestUri);
            mockRequest.Setup(r => r.QueryString).Returns(new NameValueCollection
            {
                {"uri", "http://example.com/platibus"},
                {"ttl", "3600"}
            });

            var mockResponse = new Mock<IHttpResourceResponse>();

            await controller.Process(mockRequest.Object, mockResponse.Object, new[] { "topic-1", "subscriber", encodedSubscriberUri });

            mockResponse.VerifySet(r => r.StatusCode = 202);
            mockSubscriptionTrackingService.Verify(ts => ts.AddSubscription(
                "topic-1", new Uri("http://example.com/platibus"), TimeSpan.FromSeconds(3600),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task ValidDeleteRequestToTopicSubscriberResourceRemovesSubscription()
        {
            var mockSubscriptionTrackingService = new Mock<ISubscriptionTrackingService>();
            mockSubscriptionTrackingService.Setup(sts =>
                sts.RemoveSubscription(It.IsAny<TopicName>(), It.IsAny<Uri>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            var topics = new TopicName[] { "topic-1", "topic-2", "topic-3" };
            var controller = new TopicController(mockSubscriptionTrackingService.Object, topics);

            var mockRequest = new Mock<IHttpResourceRequest>();
            var encodedSubscriberUri = HttpUtility.UrlEncode("http://example.com/platibus");
            var requestUri = new UriBuilder
            {
                Scheme = "http",
                Host = "localhost",
                Path = "/platibus/topic/topic-1/subscriber",
                Query = string.Format("uri={0}", encodedSubscriberUri)
            }.Uri;

            mockRequest.Setup(r => r.HttpMethod).Returns("DELETE");
            mockRequest.Setup(r => r.Url).Returns(requestUri);
            mockRequest.Setup(r => r.QueryString).Returns(new NameValueCollection
            {
                {"uri", "http://example.com/platibus"}
            });

            var mockResponse = new Mock<IHttpResourceResponse>();

            await controller.Process(mockRequest.Object, mockResponse.Object, new[] { "topic-1", "subscriber", encodedSubscriberUri });

            mockResponse.VerifySet(r => r.StatusCode = 202);
            mockSubscriptionTrackingService.Verify(ts => ts.RemoveSubscription(
                "topic-1", new Uri("http://example.com/platibus"), It.IsAny<CancellationToken>()));
        }
    }
}