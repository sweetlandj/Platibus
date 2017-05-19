using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Platibus.Config;
using Platibus.Http;
using Platibus.InMemory;

namespace Platibus.UnitTests.Http
{
    [Trait("Category", "UnitTests")]
    public class HttpTransportServiceTests
    {
        [Fact]
        public async Task MessageCanBePostedToRemote()
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

                var transportService = server.TransportService;
                await transportService.SendMessage(message);

                messageReceived = await messageReceivedEvent
                    .WaitOneAsync(TimeSpan.FromSeconds(3));
            }

            // Sanity check.  We're really testing the transport to ensure
            // that it doesn't throw.  But if it does throw, then it would
            // be nice to get some info about how the server behaved.
            Assert.True(messageReceived);
            Assert.Equal("Hello, world!", content);
        }

        [Fact]
        public async Task TransportExceptionThrownIfConnectionRefused()
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

            await Assert.ThrowsAsync<ConnectionRefusedException>(() => transportService.SendMessage(message));
        }

        [Fact]
        public async Task TransportExceptionThrownIfNameResolutionFails()
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

            await Assert.ThrowsAsync<NameResolutionFailedException>(() => transportService.SendMessage(message));
        }
    }
}