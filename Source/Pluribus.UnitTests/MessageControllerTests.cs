using NUnit.Framework;
using Pluribus.Http;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using Moq;
using System.Collections.Specialized;
using System.Security.Principal;

namespace Pluribus.UnitTests
{
    class MessageControllerTests
    {
        [Test]
        public async Task Given_Content_When_Posting_Then_Transport_Service_Accepts_Message()
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
            var mockTransportService = new Mock<ITransportService>();
            var controller = new MessageController(mockTransportService.Object);

            var messageReceivedEvent = new ManualResetEvent(false);
            Message receivedMessage = null;
            mockTransportService.Setup(x => x.AcceptMessage(It.IsAny<Message>(), It.IsAny<IPrincipal>(), It.IsAny<CancellationToken>()))
                .Callback<Message, IPrincipal, CancellationToken>((m, p, ct) =>
                {
                    receivedMessage = m;
                    messageReceivedEvent.Set();
                })
                .Returns(Task.FromResult(true));

            await controller
                .Process(mockRequest.Object, mockResponse.Object, Enumerable.Empty<string>())
                .ConfigureAwait(false);

            var messageReceived = messageReceivedEvent.WaitOne(TimeSpan.FromSeconds(1));

            Assert.That(messageReceived, Is.True);
            Assert.That(receivedMessage, Is.Not.Null);
            Assert.That(receivedMessage.Content, Is.EqualTo("Hello, world!"));
            Assert.That(receivedMessage.Headers.MessageId, Is.EqualTo(messageId));

            mockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.Accepted);

            Console.WriteLine(receivedMessage.Content);
        }

    }
}
