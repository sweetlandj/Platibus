using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Platibus.Http;

namespace Platibus.UnitTests.Http
{
    internal class MessageControllerTests
    {
        [Fact]
        public async Task ValidPostRequestToMessageResourceAccepted()
        {
            var messageId = MessageId.Generate();

            var mockRequest = new Mock<IHttpResourceRequest>();

            var entityBody = Encoding.UTF8.GetBytes("Hello, world!");
            var entityBodyStream = new MemoryStream(entityBody);
            mockRequest.Setup(r => r.HttpMethod).Returns("POST");
            mockRequest.Setup(r => r.InputStream).Returns(entityBodyStream);
            mockRequest.Setup(r => r.ContentType).Returns("text/plain");
            mockRequest.Setup(r => r.ContentEncoding).Returns(Encoding.UTF8);
            mockRequest.Setup(r => r.Headers)
                .Returns(new NameValueCollection
                {
                    {HeaderName.MessageId, messageId}
                });

            var mockResponse = new Mock<IHttpResourceResponse>();

            var messageReceivedEvent = new ManualResetEvent(false);
            Message receivedMessage = null;
            var controller = new MessageController((m, p) =>
            {
                receivedMessage = m;
                messageReceivedEvent.Set();
                return Task.FromResult(true);
            });

            await controller.Process(mockRequest.Object, mockResponse.Object, Enumerable.Empty<string>());

            var messageReceived = messageReceivedEvent.WaitOne(TimeSpan.FromSeconds(1));

            Assert.True(messageReceived);
            Assert.NotNull(receivedMessage);
            Assert.Equal("Hello, world!", receivedMessage.Content);
            Assert.Equal(messageId, receivedMessage.Headers.MessageId);

            mockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.Accepted);

            Console.WriteLine(receivedMessage.Content);
        }
    }
}