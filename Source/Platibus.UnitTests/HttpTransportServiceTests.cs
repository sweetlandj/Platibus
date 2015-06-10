using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Platibus.Config;
using Platibus.Http;
using Platibus.InMemory;

namespace Platibus.UnitTests
{
    internal class HttpTransportServiceTests
    {
        [Test]
        public async Task Given_Valid_Request_And_Remote_Server_Listening_When_Sending_Then_Remote_Accepts()
        {
            var messageReceivedEvent = new ManualResetEvent(false);
            string content = null;

            var serverBaseUri = new UriBuilder
            {
                Scheme = "http",
                Host = "localhost",
                Port = 52180,
                Path = "/platibus.test/"
            }.Uri;

            var configuration = new HttpServerConfiguration
            {
                BaseUri = serverBaseUri
            };

            configuration.AddHandlingRule<string>(".*", (c, ctx) =>
            {
                content = c;
                messageReceivedEvent.Set();
                ctx.Acknowledge();
            });

            bool messageReceived;
            using (var server = await HttpServer.Start(configuration))
            {
                var endpoint = serverBaseUri;
                var message = new Message(new MessageHeaders
                {
                    {HeaderName.ContentType, "text/plain"},
                    {HeaderName.MessageId, Guid.NewGuid().ToString()},
                    {HeaderName.Destination, endpoint.ToString()},
                    {HeaderName.MessageName, typeof (string).FullName},
                }, "Hello, world!");

                var transportService = await server.GetTransportService();
                await transportService.SendMessage(message);

                messageReceived = await messageReceivedEvent
                    .WaitOneAsync(TimeSpan.FromSeconds(3));
            }

            // Sanity check.  We're really testing the transport to ensure
            // that it doesn't throw.  But if it does throw, then it would
            // be nice to get some info about how the server behaved.
            Assert.That(messageReceived, Is.True);
            Assert.That(content, Is.EqualTo("Hello, world!"));
        }

        [Test]
        public async Task Given_Remote_Host_Not_Listening_When_Sending_Then_Transport_Service_Throws_TransportException()
        {
            var endpoint = new UriBuilder
            {
                Scheme = "http",
                Host = "localhost",
                Port = 52180,
                Path = "/platibus.test/"
            }.Uri;

            var transportService = new HttpTransportService(endpoint, ReadOnlyEndpointCollection.Empty, new InMemoryMessageQueueingService(), null, new InMemorySubscriptionTrackingService());

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()},
                {HeaderName.Destination, endpoint.ToString()}
            }, "Hello, world!");

            Exception exception = null;
            try
            {
                await transportService.SendMessage(message);
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
            Assert.That(exception, Is.InstanceOf<ConnectionRefusedException>());
        }

        [Test]
        public async Task Given_Invalid_Hostname_When_Sending_Then_Transport_Service_Throws_TransportException()
        {
            var endpoint = new UriBuilder
            {
                Scheme = "http",
                Host = "dne.example.test",
                Port = 52180,
                Path = "/platibus.test/"
            }.Uri;

            var transportService = new HttpTransportService(endpoint, ReadOnlyEndpointCollection.Empty, new InMemoryMessageQueueingService(), null, new InMemorySubscriptionTrackingService());

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()},
                {HeaderName.Destination, endpoint.ToString()}
            }, "Hello, world!");

            Exception exception = null;
            try
            {
                await transportService.SendMessage(message);
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