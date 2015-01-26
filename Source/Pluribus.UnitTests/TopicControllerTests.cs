using Moq;
using NUnit.Framework;
using Pluribus.Http;
using Pluribus.Serialization;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Pluribus.UnitTests
{
    class TopicControllerTests
    {
        [Test]
        public async Task Given_No_Topic_When_Getting_Then_Topic_List_Returned()
        {
            var mockTransportService = new Mock<ITransportService>();
            var topics = new TopicName[]{"topic-1", "topic-2", "topic-3"};
            var controller = new TopicController(mockTransportService.Object, topics);

            var mockRequest = new Mock<IHttpResourceRequest>();
            var requestUri = new UriBuilder
            {
                Scheme = "http",
                Host = "localhost",
                Path = "/pluribus/topic/"
            }.Uri;
            mockRequest.Setup(r => r.HttpMethod).Returns("GET");
            mockRequest.Setup(r => r.Url).Returns(requestUri);
            
            var mockResponse = new Mock<IHttpResourceResponse>();
            var responseOutputStream = new MemoryStream();
            var responseEncoding = Encoding.UTF8;
            mockResponse.Setup(r => r.ContentEncoding).Returns(responseEncoding);
            mockResponse.Setup(r => r.OutputStream).Returns(responseOutputStream);

            await controller
                .Process(mockRequest.Object, mockResponse.Object, Enumerable.Empty<string>())
                .ConfigureAwait(false);

            mockResponse.VerifySet(r => r.StatusCode = 200);
            var responseContent = responseEncoding.GetString(responseOutputStream.GetBuffer());
            var topicsFromResponse = new NewtonsoftJsonSerializer().Deserialize<string[]>(responseContent);

            Assert.That(topicsFromResponse.Length, Is.EqualTo(3));
            Assert.That(topicsFromResponse, Contains.Item("topic-1"));
            Assert.That(topicsFromResponse, Contains.Item("topic-2"));
            Assert.That(topicsFromResponse, Contains.Item("topic-3"));
        }

        [Test]
        public async Task Given_No_Topic_When_Posting_Then_400_Returned()
        {
            var mockTransportService = new Mock<ITransportService>();
            var topics = new TopicName[] { "topic-1", "topic-2", "topic-3" };
            var controller = new TopicController(mockTransportService.Object, topics);

            var mockRequest = new Mock<IHttpResourceRequest>();
            var requestUri = new UriBuilder
            {
                Scheme = "http",
                Host = "localhost",
                Path = "/pluribus/topic/"
            }.Uri;
            mockRequest.Setup(r => r.HttpMethod).Returns("POST");
            mockRequest.Setup(r => r.Url).Returns(requestUri);

            var mockResponse = new Mock<IHttpResourceResponse>();
            var responseOutputStream = new MemoryStream();
            var responseEncoding = Encoding.UTF8;
            mockResponse.Setup(r => r.ContentEncoding).Returns(responseEncoding);
            mockResponse.Setup(r => r.OutputStream).Returns(responseOutputStream);

            await controller
                .Process(mockRequest.Object, mockResponse.Object, Enumerable.Empty<string>())
                .ConfigureAwait(false);

            mockResponse.VerifySet(r => r.StatusCode = 400);
        }

        [Test]
        public async Task Given_Topic_And_Subscriber_When_Posting_Then_Subscription_Added()
        {
            var mockTransportService = new Mock<ITransportService>();
            mockTransportService.Setup(ts => ts.AcceptSubscriptionRequest(It.IsAny<SubscriptionRequestType>(),
                It.IsAny<TopicName>(), It.IsAny<Uri>(), It.IsAny<TimeSpan>(), It.IsAny<IPrincipal>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            var topics = new TopicName[] { "topic-1", "topic-2", "topic-3" };
            var controller = new TopicController(mockTransportService.Object, topics);

            var mockRequest = new Mock<IHttpResourceRequest>();
            var encodedSubscriberUri = HttpUtility.UrlEncode("http://example.com/pluribus");
            var requestUri = new UriBuilder
            {
                Scheme = "http",
                Host = "localhost",
                Path = "/pluribus/topic/topic-1/subscriber",
                Query = string.Format("uri={0}&ttl=3600", encodedSubscriberUri)
            }.Uri;

            mockRequest.Setup(r => r.HttpMethod).Returns("POST");
            mockRequest.Setup(r => r.Url).Returns(requestUri);
            mockRequest.Setup(r => r.QueryString).Returns(new NameValueCollection
            {
                {"uri", "http://example.com/pluribus"},
                {"ttl", "3600"}
            });

            var mockResponse = new Mock<IHttpResourceResponse>();
            
            await controller
                .Process(mockRequest.Object, mockResponse.Object, new [] {"topic-1", "subscriber", encodedSubscriberUri})
                .ConfigureAwait(false);

            mockResponse.VerifySet(r => r.StatusCode = 202);
            mockTransportService.Verify(ts => ts.AcceptSubscriptionRequest(SubscriptionRequestType.Add,
                "topic-1", new Uri("http://example.com/pluribus"), TimeSpan.FromSeconds(3600), It.IsAny<IPrincipal>(), 
                It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Given_Topic_And_Subscriber_When_Deleting_Then_Subscription_Removed()
        {
            var mockTransportService = new Mock<ITransportService>();
            mockTransportService.Setup(ts => ts.AcceptSubscriptionRequest(It.IsAny<SubscriptionRequestType>(),
                It.IsAny<TopicName>(), It.IsAny<Uri>(), It.IsAny<TimeSpan>(), It.IsAny<IPrincipal>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            var topics = new TopicName[] { "topic-1", "topic-2", "topic-3" };
            var controller = new TopicController(mockTransportService.Object, topics);

            var mockRequest = new Mock<IHttpResourceRequest>();
            var encodedSubscriberUri = HttpUtility.UrlEncode("http://example.com/pluribus");
            var requestUri = new UriBuilder
            {
                Scheme = "http",
                Host = "localhost",
                Path = "/pluribus/topic/topic-1/subscriber",
                Query = string.Format("uri={0}", encodedSubscriberUri)
            }.Uri;

            mockRequest.Setup(r => r.HttpMethod).Returns("DELETE");
            mockRequest.Setup(r => r.Url).Returns(requestUri);
            mockRequest.Setup(r => r.QueryString).Returns(new NameValueCollection
            {
                {"uri", "http://example.com/pluribus"}
            });

            var mockResponse = new Mock<IHttpResourceResponse>();

            await controller
                .Process(mockRequest.Object, mockResponse.Object, new[] { "topic-1", "subscriber", encodedSubscriberUri })
                .ConfigureAwait(false);

            mockResponse.VerifySet(r => r.StatusCode = 202);
            mockTransportService.Verify(ts => ts.AcceptSubscriptionRequest(SubscriptionRequestType.Remove,
                "topic-1", new Uri("http://example.com/pluribus"), TimeSpan.Zero, It.IsAny<IPrincipal>(),
                It.IsAny<CancellationToken>()));
        }
    }
}
