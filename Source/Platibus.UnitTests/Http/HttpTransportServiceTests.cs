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
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Platibus.Config;
using Platibus.Http;
using Platibus.InMemory;
using Platibus.Utils;

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
                Port = 53061,
                Path = "/platibus.test/"
            }.Uri;

            bool messageReceived;
            using (var server = HttpServer.Start(configuration =>
            {
                configuration.BaseUri = serverBaseUri;
                configuration.AddHandlingRule<string>(".*", (c, ctx) =>
                {
                    content = c;
                    messageReceivedEvent.Set();
                    ctx.Acknowledge();
                });
            }))
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
                Port = 53062,
                Path = "/platibus.test/"
            }.Uri;

            var transportServiceOptions = new HttpTransportServiceOptions(endpoint, new InMemoryMessageQueueingService(), new InMemorySubscriptionTrackingService());
            var transportService = new HttpTransportService(transportServiceOptions);

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()},
                {HeaderName.Destination, endpoint.ToString()},
                {HeaderName.Synchronous, "true"}
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
                Port = 53063,
                Path = "/platibus.test/"
            }.Uri;

            var transportServiceOptions = new HttpTransportServiceOptions(endpoint, new InMemoryMessageQueueingService(), new InMemorySubscriptionTrackingService());
            var transportService = new HttpTransportService(transportServiceOptions);

            var message = new Message(new MessageHeaders
            {
                {HeaderName.ContentType, "text/plain"},
                {HeaderName.MessageId, Guid.NewGuid().ToString()},
                {HeaderName.Destination, endpoint.ToString()},
                {HeaderName.Synchronous, "true"}
            }, "Hello, world!");

            await Assert.ThrowsAsync<NameResolutionFailedException>(() => transportService.SendMessage(message));
        }
    }
}