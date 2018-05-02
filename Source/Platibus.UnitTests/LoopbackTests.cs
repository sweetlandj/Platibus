using System;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Config;
using Platibus.InMemory;
using Xunit;

namespace Platibus.UnitTests
{
    public class LoopbackTests
    {
        protected LoopbackConfiguration Configuration = new LoopbackConfiguration();
        protected object MessageContent = Guid.NewGuid().ToString();
        protected SendOptions SendOptions = new SendOptions {ContentType = "text/plain"};
        protected CancellationTokenSource CancellationSource = new CancellationTokenSource();

        public LoopbackTests()
        {
            Configuration.MessageQueueingService = new InMemoryMessageQueueingService();
        }

        [Theory]
        [InlineData("loopback")] // Standard loopback endpoint
        [InlineData("undefined")] // Undefined/unknown endpoint
        public async Task MessageIsHandledWhenSendingToNamedEndpoint(string endpointName)
        {
            var handlerTask = GivenHandler();
            await WhenSendingToNamedEndpoint(endpointName);
            var handledContent = await handlerTask;
            Assert.Equal(MessageContent, handledContent);
        }

        [Theory]
        [InlineData("urn:localhost/loopback")] // Standard loopback endpoint address
        [InlineData("http://localhost/undefined")] // Undefined/unknown endpoint address
        public async Task MessageIsHandledWhenSendingToEndpointAddress(string address)
        {
            var handlerTask = GivenHandler();
            await WhenSendingToEndpointAddress(new Uri(address));
            var handledContent = await handlerTask;
            Assert.Equal(MessageContent, handledContent);
        }

        protected Task<object> GivenHandler()
        {
            var handlerCompletionSource = new TaskCompletionSource<object>();
            CancellationSource.Token.Register(() => handlerCompletionSource.TrySetCanceled());
            Configuration.AddHandlingRule<object>(".*", (content, ctx) =>
                {
                    handlerCompletionSource.TrySetResult(content);
                    ctx.Acknowledge();
                });
            return handlerCompletionSource.Task;
        }

        protected async Task WhenSendingToNamedEndpoint(EndpointName endpointName)
        {
            var host = LoopbackHost.Start(Configuration, CancellationSource.Token);
            await host.Bus.Send(MessageContent, endpointName, SendOptions, CancellationSource.Token);
        }

        protected async Task WhenSendingToEndpointAddress(Uri endpointAddress)
        {
            var host = LoopbackHost.Start(Configuration, CancellationSource.Token);
            await host.Bus.Send(MessageContent, endpointAddress, SendOptions, CancellationSource.Token);
        }
    }
}
