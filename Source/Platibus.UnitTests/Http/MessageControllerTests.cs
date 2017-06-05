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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Platibus.Http;
using Platibus.Http.Controllers;

namespace Platibus.UnitTests.Http
{
    [Trait("Category", "UnitTests")]
    public class MessageControllerTests
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