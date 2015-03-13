using Moq;
using NUnit.Framework;
using Platibus.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.UnitTests
{
    class HttpTransportServiceTests
    {
        [Test]
        public async Task Given_Valid_Request_And_Remote_Server_Listening_When_Sending_Then_Remote_Accepts()
        {
            var messageReceivedEvent = new ManualResetEvent(false);
            string content = null; 

            var transportService = new HttpTransportService();
            var mockMessageController = new Mock<IHttpResourceController>();
            mockMessageController.Setup(c => c.Process(It.IsAny<IHttpResourceRequest>(), It.IsAny<IHttpResourceResponse>(), It.IsAny<IEnumerable<string>>()))
                .Callback<IHttpResourceRequest, IHttpResourceResponse, IEnumerable<string>>((req, resp, subPath) =>
                {
                    content = req.ReadContentAsString().Result;
                    messageReceivedEvent.Set();
                })
                .Returns(Task.FromResult(true));

            var router = new ResourceTypeDictionaryRouter
            {
                {"message", mockMessageController.Object}
            };

            var serverBaseUri = new UriBuilder
            {
                Scheme = "http",
                Host = "localhost",
                Port = 52180,
                Path = "/platibus.test/"
            }.Uri;

            bool messageReceived;
            using(var server = new HttpServer(serverBaseUri, router))
            {
                server.Start();

                var endpoint = serverBaseUri;
                var message = new Message(new MessageHeaders
                {
                    {HeaderName.ContentType, "text/plain"},
                    {HeaderName.MessageId, Guid.NewGuid().ToString()},
                    {HeaderName.Destination, endpoint.ToString()}
                }, "Hello, world!");

                await transportService
                    .SendMessage(message)
                    .ConfigureAwait(false);

                messageReceived = await messageReceivedEvent
                    .WaitOneAsync(TimeSpan.FromSeconds(3))
                    .ConfigureAwait(false);                
            }

            // Sanity check.  We're really testing the transport to ensure
            // that it doesn't throw.  But if it does throw, then it would
            // be nice to get some info about how the server behaved.
            Assert.That(messageReceived, Is.True);
            Assert.That(content, Is.EqualTo("Hello, world!"));
        }

        [Test]
        public async Task Given_Invalid_Resource_Type_When_Sending_Then_Remote_Responds_Invalid_Request()
        {
            var messageReceivedEvent = new ManualResetEvent(false);
            var transportService = new HttpTransportService();
            var mockMessageController = new Mock<IHttpResourceController>();
            mockMessageController.Setup(c => c.Process(It.IsAny<IHttpResourceRequest>(), It.IsAny<IHttpResourceResponse>(), It.IsAny<IEnumerable<string>>()))
                .Callback<IHttpResourceRequest, IHttpResourceResponse, IEnumerable<string>>((req, resp, subPath) =>
                {
                    resp.StatusCode = 400;
                    messageReceivedEvent.Set();
                })
                .Returns(Task.FromResult(true));

            // Similate a configuration in which the server does not
            // know what a "message" resource is.
            var router = new ResourceTypeDictionaryRouter();

            var serverBaseUri = new UriBuilder
            {
                Scheme = "http",
                Host = "localhost",
                Port = 52180,
                Path = "/platibus.test/"
            }.Uri;

            Exception exception = null;
            using (var server = new HttpServer(serverBaseUri, router))
            {
                server.Start();

                var endpoint = serverBaseUri;
                var message = new Message(new MessageHeaders
                {
                    {HeaderName.ContentType, "text/plain"},
                    {HeaderName.MessageId, Guid.NewGuid().ToString()},
                    {HeaderName.Destination, endpoint.ToString()}
                }, "Hello, world!");

                try
                {
                    await transportService
                        .SendMessage(message)
                        .ConfigureAwait(false);    
                }
                catch(AggregateException aex)
                {
                    exception = aex.InnerExceptions.FirstOrDefault();
                }
                catch(Exception ex)
                {
                    exception = ex;
                }
                await messageReceivedEvent
                    .WaitOneAsync(TimeSpan.FromSeconds(3))
                    .ConfigureAwait(false);
            }

            // Sanity check.  We're really testing the transport to ensure
            // that it doesn't throw.  But if it does throw, then it would
            // be nice to get some info about how the server behaved.
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception, Is.InstanceOf<InvalidRequestException>());
        }

        [Test]
        public async Task Given_Remote_Host_Not_Listening_When_Sending_Then_Transport_Service_Throws_TransportException()
        {
            var transportService = new HttpTransportService();

            var endpoint = new UriBuilder
            {
                Scheme = "http",
                Host = "localhost",
                Port = 52180,
                Path = "/platibus.test/"
            }.Uri;

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()},
                {HeaderName.Destination, endpoint.ToString()}
            }, "Hello, world!");

            Exception exception = null;
            try
            {
                await transportService
                    .SendMessage(message)
                    .ConfigureAwait(false);
            }
            catch(AggregateException ae)
            {
                exception = ae.InnerExceptions.FirstOrDefault();
            }
            catch(Exception ex)
            {
                exception = ex;
            }
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception, Is.InstanceOf<ConnectionRefusedException>());
        }

        [Test]
        public async Task Given_Invalid_Hostname_When_Sending_Then_Transport_Service_Throws_TransportException()
        {
            var transportService = new HttpTransportService();

            var endpoint = new UriBuilder
            {
                Scheme = "http",
                Host = "dne.example.test",
                Port = 52180,
                Path = "/platibus.test/"
            }.Uri;

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()},
                {HeaderName.Destination, endpoint.ToString()}
            }, "Hello, world!");

            Exception exception = null;
            try
            {
                await transportService
                    .SendMessage(message)
                    .ConfigureAwait(false);
            }
            catch (AggregateException ae)
            {
                exception = ae.InnerExceptions.FirstOrDefault();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception, Is.InstanceOf<NameResolutionFailedException>());
        }
    }
}
